using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Movement : MonoBehaviour
{

    [Header("Player Inputs")]
    public PlayerInput playerControls;

    [Header("Movement")]
    public GameObject GroundCheck;
    public float GroundDistance = 0.2f;
    public LayerMask GroundMask;
    public float MoveSpeed = 5f;
    public float MaxFallSpeed = 100f;
    public bool ignoreGravity = false;
    public float GroundDrag = 6f;
    public float AirDrag = 2f;
    private float GroundGravity = 2f;


    private Rigidbody rb;
    private Vector2 MoveDirection = Vector2.zero;
    private Vector3 Move = Vector3.zero;
    public bool isGrounded { get; private set; }
    private InputAction MoveController;
    private RaycastHit slopeHit;
    private float playerHeight;
    private Vector3 slopeMoveDirection;


    [Header("Camera Controls")]
    public Transform playerBody;
    public GameObject camera;
    public float X_AimSensitivity;
    public float Y_AimSensitivity;

    private Vector2 MousePosition;
    private float X_Rotation = 0;
    private InputAction Look;

    [Header("Jumping")]
    public float jumpHeight = 8f;
    public float Gravity = -9.8f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    private InputAction jumpInput;
    private bool shouldJump = false;
    private bool jumpButtonIsHeld = false;

    private Vector3 newrayCastPosition;

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void OnEnable()
    {
        playerControls.ActivateInput();

        MoveController = playerControls.actions["Move"];
        Look = playerControls.actions["Look"];
        jumpInput = playerControls.actions["Jump"];

        MoveController.Enable();
        Look.Enable();

        jumpInput.started += jumpButtonPressed;
        jumpInput.canceled += jumpRelease;

        Cursor.lockState = CursorLockMode.Locked;

        rb = gameObject.GetComponent<Rigidbody>();
        rb.drag = GroundDrag;
        playerHeight = gameObject.GetComponent<CapsuleCollider>().height;
    }

    public void FixedUpdate()
    {
        CheckOnGround();
        GatherInputs();
        newrayCastPosition = transform.position + (Move * 0.5f);
        HandleCamera();
        Jump();
        ApplyGravity();
        ApplyDrag();
        MovePlayer();
        ClampSpeed();
    }
    private void OnDisable()
    {
        playerControls.DeactivateInput();
        MoveController.Disable();
        Look.Disable();
    }

    private void ApplyDrag()
    {
        rb.drag = isGrounded ? GroundDrag : AirDrag;
    }

    public void CheckOnGround()
    {
        Vector3 slopeNormal = getSlope();
        float slopeyNess = Vector3.Dot(slopeNormal, transform.up);
        print($"slopeNormal: {slopeNormal} and slopeyNess: {slopeyNess}");
        isGrounded = Physics.CheckSphere(GroundCheck.transform.position, GroundDistance, GroundMask) && slopeyNess > 0.5;
    }
    private void ClampSpeed()
    {

        float x = Mathf.Clamp(rb.velocity.x, -MoveSpeed, MoveSpeed);
        float z = Mathf.Clamp(rb.velocity.z, -MoveSpeed, MoveSpeed);
        float y = Mathf.Clamp(rb.velocity.y, -MaxFallSpeed, MaxFallSpeed);

        if (!isGrounded)
        {
            x = rb.velocity.x;
            z = rb.velocity.z;
        }

        rb.velocity = new Vector3(x, y, z);
    }
    private void ApplyGravity()
    {
        if (ignoreGravity)
        {
            return;
        }

        float gravityApplied = Gravity;

        if (rb.velocity.y < 0 && !isGrounded)
        {
            gravityApplied *= fallMultiplier;
        }
        else if (rb.velocity.y > 0 && !jumpButtonIsHeld)
        {
            gravityApplied *= lowJumpMultiplier;
        }

        rb.AddForce(transform.up * gravityApplied, ForceMode.Acceleration);
    }
    private void Jump()
    {
        if (shouldJump)
        {
            shouldJump = false;
            jumpButtonIsHeld = true;
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
        }
    }
    private bool OnSlope()
    {
        if (Physics.Raycast(newrayCastPosition, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            return slopeHit.normal != Vector3.up;
        }
        return false;
    }

    private Vector3 getSlope()
    {
        Physics.Raycast(newrayCastPosition, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f);
        return slopeHit.normal;
    }

    private void MovePlayer()
    {
        Move = (transform.forward * MoveDirection.y) + (transform.right * MoveDirection.x);
        slopeMoveDirection = Vector3.ProjectOnPlane(Move, slopeHit.normal);


        if (OnSlope())
        {
            Vector3 slopeForce = slopeMoveDirection.normalized;
            slopeForce.y *= 1.2f;
            rb.AddForce(slopeForce * MoveSpeed, ForceMode.Acceleration);
        }
        else
        {
            Vector3 normalWalkForce = new Vector3(Move.x, 0 , Move.z);
            rb.AddForce(normalWalkForce * MoveSpeed, ForceMode.Acceleration);
        }
    }
    private void HandleCamera()
    {
        float Look_X = MousePosition.x * X_AimSensitivity;
        float Look_Y = MousePosition.y * Y_AimSensitivity;
        X_Rotation -= Look_Y;
        X_Rotation = Mathf.Clamp(X_Rotation, -90f, 90f);
        playerBody.Rotate(Vector3.up * Look_X);
        camera.transform.localRotation = Quaternion.Euler(X_Rotation, 0f, 0f);

    }
    private void GatherInputs()
    {
        MoveDirection = MoveController.ReadValue<Vector2>();
        MousePosition = Look.ReadValue<Vector2>();
    }
    private void jumpButtonPressed(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            shouldJump = true;
        }
    }
    private void jumpRelease(InputAction.CallbackContext obj)
    {
        jumpButtonIsHeld = false;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(newrayCastPosition, 0.1f);
    }

}


