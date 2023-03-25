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
    public bool ignoreGravity = false;


    private Rigidbody rb;
    private Vector2 MoveDirection = Vector2.zero;
    private Vector3 Move = Vector3.zero;
    public Vector3 Velocity;
    private bool isGrounded;
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
        playerHeight = gameObject.GetComponent<CapsuleCollider>().height;
    }



    // Fixed Update is called before Update
    public void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(GroundCheck.transform.position, GroundDistance, GroundMask);
        GatherInputs();
        HandleCamera();

        if (!ignoreGravity)
        {
            rb.AddForce(new Vector3(0f, Gravity, 0f));
        }

        Jump();

        slopeMoveDirection = Vector3.ProjectOnPlane(Move, slopeHit.normal);

        MovePlayer();
    }
    public void Update()
    {
    }

    private void ClampSpeed()
    {
        Move.y = Mathf.Clamp(Move.y, -MaxFallSpeed, +MaxFallSpeed);
        Move.x = Mathf.Clamp(Move.x, -MoveSpeed, MoveSpeed);
        Move.z = Mathf.Clamp(Move.z, -MoveSpeed, MoveSpeed);
    }

    private void jumpButtonPressed(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            shouldJump = true;
        }
    }
    private void Jump()
    {
        if (Move.y < 0 && !isGrounded)
        {
            Move.y -= fallMultiplier;
        }
        else if (Move.y < 0 && isGrounded)
        {
            Move.y = -2f;
        }
        else if (Move.y > 0 && !jumpButtonIsHeld)
        {
            Move.y -= lowJumpMultiplier;
        }

        if (shouldJump)
        {
            shouldJump = false;
            jumpButtonIsHeld = true;
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
            ClampSpeed();
        }
        else
        {
            rb.AddForce(new Vector3(0, Move.y, 0), ForceMode.Force);
        }
    }

    private void jumpRelease(InputAction.CallbackContext obj)
    {
        jumpButtonIsHeld = false;
    }

    private void OnDisable()
    {
        playerControls.DeactivateInput();
        MoveController.Disable();
        Look.Disable();
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            return slopeHit.normal != Vector3.up;
        }
        return false;
    }

    private void MovePlayer()
    {
        //Vector2 xMove = new Vector2(MoveDirection.x * transform.right.x, MoveDirection.x * transform.right.z);
        //Vector2 yMove = new Vector2(MoveDirection.y * transform.forward.x, MoveDirection.y * transform.forward.z);
        //Vector2 velocity = (xMove + yMove).normalized * MoveSpeed;
        //Move = new Vector3(velocity.x, rb.velocity.y, velocity.y);

        Move = (transform.forward * MoveDirection.y) + (transform.right * MoveDirection.x);

        if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDirection.normalized * MoveSpeed, ForceMode.Acceleration);
            print($"On Slope: Normalized Directions : {slopeMoveDirection.normalized}");
        }
        else if (isGrounded && !OnSlope())
        {
            rb.AddForce(Move * MoveSpeed, ForceMode.Acceleration);
            print($"Off Slope: Directions : {Move}");
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


}
