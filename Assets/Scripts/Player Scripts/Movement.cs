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
    public Vector3 Velocity;
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
    }
    private void OnEnable()
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

    private void OnDisable()
    {
        playerControls.DeactivateInput();
        MoveController.Disable();
        Look.Disable();
    }

}
