using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Movement : MonoBehaviour
{

    [Header("Player Inputs and Controller")]
    public CharacterController cc;
    public PlayerInput playerControls;

    [Header("Movement")]
    public GameObject GroundCheck;
    public float GroundDistance = 0.4f;
    public LayerMask GroundMask;
    public float MoveSpeed = 5f;
    public float MaxFallSpeed = 100f;

    private Vector2 MoveDirection = Vector2.zero;
    private Vector3 Velocity;
    private bool isGrounded;
    private InputAction MoveController;

    [Header("Camera Controls")]
    public Transform playerBody;
    public GameObject camera;
    public float X_AimSensitivity;
    public float Y_AimSensitivity;

    private Vector2 MousePosition;
    private float X_Rotation = 0;
    private InputAction Look;

    [Header("Torch Controls")]
    public GameObject Torch;
    public GameObject TorchSpawnPoint;
    public float torchUpwardsForce = 5.5f, torchForwardForce = 8f;
    public float TorchForceMultiplier = 2f;
    public GameObject HeldTorch;

    private bool isTorchButtonHeldDown = false;
    private Vector3 DefaultTorchPosition;
    private InputAction TossInput;
    private float DefaultForwardsForce;
    private float torchDistanceMoved = 0;

    [Header("Whip Controls")]
    public float whipCoolDownTime = 1f;
    public float whipRange = 5f;
    public bool didWhipHit = false;
    public float whipAnimationSpeed = 1f;
    public GameObject whipBase;
    public float whipArchHeight = 0.5f;
    public float whipHitColliderRadius = 0.5f;
    public GameObject whipHitCollider;

    private bool canFireWhip = true;
    private InputAction WhipInput;
    private Vector3 whipHitPosition;
    private RaycastHit hit;
    private float hitDistance;
    private LineRenderer lineRenderer;
    private bool shouldAnimateWhip = false;
    private Vector3 currentWhipTipPosition;

    [Header("Jumping")]
    public float jumpHeight = 8f;
    public float Gravity = -9.8f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    private InputAction jumpInput;
    private Rigidbody rb;
    private bool shouldJump = false;
    private bool jumpButtonIsHeld = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        DefaultForwardsForce = torchForwardForce;
        DefaultTorchPosition = HeldTorch.transform.localPosition;
    }
    private void OnEnable()
    {
        playerControls.ActivateInput();

        MoveController = playerControls.actions["Move"];
        Look = playerControls.actions["Look"];
        TossInput = playerControls.actions["Toss"];
        WhipInput = playerControls.actions["Fire"];
        jumpInput = playerControls.actions["Jump"];

        MoveController.Enable();
        Look.Enable();
        TossInput.Enable();

        TossInput.started += ChargingTorch;
        TossInput.canceled += TossTorch;
        WhipInput.started += useWhip;
        jumpInput.started += jumpButtonPressed;
        jumpInput.canceled += jumpRelease;

        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        //Movement setup
        MoveDirection = MoveController.ReadValue<Vector2>();
        MousePosition = Look.ReadValue<Vector2>();
        isGrounded = Physics.CheckSphere(GroundCheck.transform.position, GroundDistance, GroundMask);
        // Looking stuff \\
        float Look_X = MousePosition.x * X_AimSensitivity;
        float Look_Y = MousePosition.y * Y_AimSensitivity;
        X_Rotation -= Look_Y;
        X_Rotation = Mathf.Clamp(X_Rotation, -90f, 90f);

        // Rotates entire Player object left and right \\
        playerBody.Rotate(Vector3.up * Look_X);

        // Allow the camera to look up and down \\
        camera.transform.localRotation = Quaternion.Euler(X_Rotation, 0f, 0f);
        WhileChargingTorchThrow();

        // Determining jump altering logic \\
        if (Velocity.y < 0 && !isGrounded)
        {
            Velocity.y -= fallMultiplier;
        }
        else if (Velocity.y < 0 && isGrounded)
        {
            Velocity.y = -2f;
        } else if (Velocity.y >0 && !jumpButtonIsHeld)
        {
            Velocity.y -= lowJumpMultiplier;
        }

        if (shouldJump)
        {
            jump();
        }
        Velocity.y += Gravity * Time.deltaTime;
        Velocity.y = Mathf.Clamp(Velocity.y,-MaxFallSpeed,+MaxFallSpeed);
        cc.Move(Velocity * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        // Moves the Player around \\
        Vector3 Move = transform.right * MoveDirection.x + transform.forward * MoveDirection.y;
        cc.Move(Move * MoveSpeed);


        findWhipPoint();
        if (shouldAnimateWhip)
        {
            animateWhip();
        }
    }
    private void ChargingTorch(InputAction.CallbackContext obj)
    {
        isTorchButtonHeldDown = true;
    }
    private void WhileChargingTorchThrow()
    {
        float maxMoveDistance = 1f;
        print($"distanceMoved {torchDistanceMoved}");
        if (isTorchButtonHeldDown)
        {
            if (torchDistanceMoved < maxMoveDistance)
            {
                float moveDelta = 1f * Time.deltaTime;
                HeldTorch.transform.Translate(0, -moveDelta, 0, Space.World);
                torchDistanceMoved += moveDelta;
            }
        }
        else
        {
            HeldTorch.transform.localPosition = DefaultTorchPosition;
            torchDistanceMoved = 0;
        }
    }
    private void TossTorch(InputAction.CallbackContext obj)
    {
        //Direction Correction
        Vector3 forceDirection = camera.transform.forward;
        RaycastHit hit;

        if (Physics.Raycast(camera.transform.position, forceDirection, out hit, 500f))
        {
            forceDirection = (hit.point - TorchSpawnPoint.transform.position).normalized;
        }

        float heldDuration = Mathf.Min((float)obj.duration, 2.0f);

        torchForwardForce += torchForwardForce * heldDuration * TorchForceMultiplier;
        GameObject TorchInstance = Instantiate(Torch, TorchSpawnPoint.transform.position, camera.transform.rotation);
        Rigidbody torch_rb = TorchInstance.GetComponent<Rigidbody>();
        torch_rb.velocity = this.Velocity;
        Vector3 ForceToAdd = forceDirection * torchForwardForce + TorchSpawnPoint.transform.up * torchUpwardsForce;
        torch_rb.AddForce(ForceToAdd, ForceMode.Impulse);
        torch_rb.AddTorque(transform.right * Random.Range(5, 20));
        torch_rb.AddTorque(transform.up * Random.Range(-5, 5));
        torchForwardForce = DefaultForwardsForce;
        isTorchButtonHeldDown = false;
    }
    IEnumerator WhipCoolDownTimer()
    {
        yield return new WaitForSeconds(whipCoolDownTime);
        canFireWhip = true;
    }
    IEnumerator WhipAnimationTimer()
    {
        yield return new WaitForSeconds(1 / whipAnimationSpeed);
        shouldAnimateWhip = false;
    }
    private void findWhipPoint()
    {

        Vector3 camForward = camera.transform.forward;
        if (Physics.Raycast(camera.transform.position, camForward, out hit, whipRange))
        {
            Debug.DrawRay(camera.transform.position, transform.TransformDirection(Vector3.forward) * whipRange, Color.red);
            hitDistance = hit.distance;
            whipHitPosition = hit.point;
            didWhipHit = true;
        }
        else
        {
            Debug.DrawRay(camera.transform.position, transform.TransformDirection(Vector3.forward) * whipRange, Color.green);
            whipHitPosition = camera.transform.position + camera.transform.forward * whipRange;
            didWhipHit = false;
        }
    }
    private void useWhip(InputAction.CallbackContext obj)
    {
        if (canFireWhip)
        {
            string message = didWhipHit ? "Hit " + hit.collider.name : "Missed";
            Debug.Log(message);
            canFireWhip = false;
            shouldAnimateWhip = true;
            StartCoroutine(WhipCoolDownTimer());
            StartCoroutine(WhipAnimationTimer());
            if (didWhipHit)
            {
                GameObject hitCollider = Instantiate(whipHitCollider, whipHitPosition, Quaternion.identity);
            }
        }
    }
    private void jumpButtonPressed(InputAction.CallbackContext obj)
    {
     if(isGrounded)
        {
            shouldJump = true;
        }
    }
    private void jump()
    {
        Velocity.y += Mathf.Sqrt(jumpHeight * -3f * Gravity);
        shouldJump = false;
        jumpButtonIsHeld = true;
    }

    private void jumpRelease(InputAction.CallbackContext obj)
    {
        jumpButtonIsHeld = false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = didWhipHit ? Color.red : Color.green;
        Gizmos.DrawSphere(whipHitPosition, 0.1f);
        Gizmos.DrawSphere(currentWhipTipPosition, 0.1f);
    }
    private void OnDisable()
    {
        playerControls.DeactivateInput();
        MoveController.Disable();
        Look.Disable();
        TossInput.Disable();
    }
    private void animateWhip()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
        }
        Vector3[] whipPoints = new Vector3[3];
        Vector3 basePosition = whipBase.transform.position;
        Vector3 whipHitPositionLocal = transform.InverseTransformPoint(whipHitPosition);

        Vector3 peak = (whipBase.transform.position); /// 2f;
        peak = new Vector3(peak.x, peak.y + whipArchHeight, peak.z);

        whipPoints[0] = whipBase.transform.position;
        whipPoints[1] = peak;
        whipPoints[2] = whipHitPosition;

        BezierCurve curve = new BezierCurve(whipPoints);
        Vector3[] curveSegments = curve.GetSegments(10);

        //----------------------------For Eventural use with PathCreator-----------------------
        //BezierPath curve = new BezierPath(whipPoints, false, PathSpace.xyz);
        //int numOfSegments = curve.NumPoints;
        //for (int i = 0; i < numOfSegments; i++)
        //{
        //   curveSegments[i] = curve.GetPoint(i); 
        //}

        lineRenderer.positionCount = curveSegments.Length;
        lineRenderer.SetPositions(curveSegments);
    }

}
