using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Movement : MonoBehaviour
{
    //General Input Getting \\
    public CharacterController cc;
    public PlayerInput playerControls;

    // For Movement and Moving Acessories \\
    private InputAction MoveController;
    public float MoveSpeed = 5f;
    private Vector2 MoveDirection = Vector2.zero;
    private Vector3 Velocity;
    public float Gravity = -9.8f;
    public Transform GroundCheck;
    public float GroundDistance = 0.4f;
    public LayerMask GroundMask;
    private bool isGrounded;

    // For Looking \\
    private InputAction Look;
    public GameObject camera;

    public float X_AimSensitivity;
    public float Y_AimSensitivity;

    private Vector2 MousePosition;
    public Transform playerBody;
    private float X_Rotation = 0;

    // For Torch Tossing \\
    private InputAction TossInput;
    public GameObject Torch;
    public GameObject TorchSpawnPoint;
    public float torchUpwardsForce = 5.5f, torchForwardForce = 8f;
    private float DefaultForwardsForce;
    public float TorchForceMultiplier = 2f;
    public GameObject HeldTorch;
    private bool isTorchButtonHeldDown = false;
    private Vector3 DefaultTorchPosition;

    //  For The WHIP \\
    private bool canFireWhip = true;
    public float whipCoolDownTime = 1f;
    private InputAction WhipInput;
    public float whipRange = 5f;
    private Vector3 whipHitPosition;
    public bool didWhipHit = false;
    private RaycastHit hit;
    private float hitDistance;
    public float whipAnimationSpeed = 1f;
    public GameObject whipBase;
    LineRenderer lineRenderer;
    private bool shouldAnimateWhip = false;
    public float whipArchHeight = 0.5f;
    private Vector3 currentWhipTipPosition;
    public float whipHitColliderRadius = 0.5f;
    public GameObject whipHitCollider;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        DefaultForwardsForce = torchForwardForce;
        DefaultTorchPosition = HeldTorch.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        //Movement setup
        MoveDirection = MoveController.ReadValue<Vector2>();
        MousePosition = Look.ReadValue<Vector2>();
        isGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask);

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

    }

    private void FixedUpdate()
    {
        // Moves the Player around \\
        Vector3 Move = transform.right * MoveDirection.x + transform.forward * MoveDirection.y;
        cc.Move(Move * MoveSpeed);

        // Gives the Player Gravity \\
        Velocity.y += Gravity;
        cc.Move(Velocity * Time.deltaTime);

        if (isGrounded && Velocity.y < 0)
        {
            Velocity.y = -2f;
        }

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
        if (isTorchButtonHeldDown)
        {
            Vector3 CurrentTorchPosition = HeldTorch.transform.localPosition;
            HeldTorch.transform.localPosition = new Vector3(CurrentTorchPosition.x, CurrentTorchPosition.y - 0.1f, CurrentTorchPosition.z);
        }
        else
        {
            HeldTorch.transform.localPosition = DefaultTorchPosition;
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

        torchForwardForce += torchForwardForce * (float)obj.duration * TorchForceMultiplier;
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
            //Debug.Log("Missed");
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

    private void OnDrawGizmos()
    {
        Gizmos.color = didWhipHit ? Color.red : Color.green;
        Gizmos.DrawSphere(whipHitPosition, 0.1f);
        Gizmos.DrawSphere(currentWhipTipPosition, 0.1f);
    }

    private void OnEnable()
    {
        playerControls.ActivateInput();

        MoveController = playerControls.actions["Move"];
        Look = playerControls.actions["Look"];
        TossInput = playerControls.actions["Toss"];
        WhipInput = playerControls.actions["Fire"];

        MoveController.Enable();
        Look.Enable();
        TossInput.Enable();

        TossInput.started += ChargingTorch;
        TossInput.canceled += TossTorch;
        WhipInput.started += useWhip;


        Cursor.lockState = CursorLockMode.Locked;
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
