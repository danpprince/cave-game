using UnityEngine;
using UnityEngine.InputSystem;


public class Movement : MonoBehaviour
{
    //General Input Getting
    public CharacterController cc;
    public PlayerInput playerControls;

    //For Movement and Moving Acessories
    public float MoveSpeed = 5f;
    private InputAction MoveController;
    private Vector2 moveDirection = Vector2.zero;
    private Vector3 velocity;
    public float Gravity = -3.8f;
    public Transform GroundCheck;
    public float GroundDistance = 0.4f;
    public LayerMask GroundMask;
    private bool isGrounded;

    // For Looking \\
    private InputAction Look;
    public GameObject camera;  ///Make this happy.

    public float X_AimSensitivity;
    public float Y_AimSensitivity;

    private Vector2 MousePosition;
    public Transform playerBody;
    private float X_Rotation = 0;

    // for jump
    private InputAction JumpInput;
    public float jump = 10f;


    //For Torch Tossing\\
    public GameObject Torch;
    private InputAction TossInput;


    void Start()
    {
        Look = playerControls.actions["Look"];
        TossInput = playerControls.actions["Toss"];

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = MoveController.ReadValue<Vector2>();
        MousePosition = Look.ReadValue<Vector2>();
        isGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask);

        // Looking stuff \\
        float Look_X = MousePosition.x * X_AimSensitivity;
        float Look_Y = MousePosition.y * Y_AimSensitivity;
        X_Rotation -= Look_Y;
        X_Rotation = Mathf.Clamp(X_Rotation, -90f, 90f);

        //Rotates entire Player object left and right \\
        playerBody.Rotate(Vector3.up * Look_X);

        //Allow the camera to look up and down\\
        camera.transform.localRotation = Quaternion.Euler(X_Rotation, 0f, 0f);
        moveDirection = MoveController.ReadValue<Vector2>();

        
    }

    private void FixedUpdate()
    {
        //Moves the Player around
        Vector3 Move = transform.right * moveDirection.x + transform.forward * moveDirection.y;
        cc.Move(Move * MoveSpeed);
        cc.Move(velocity * Time.deltaTime);
        velocity.y += Gravity;

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        // (-0.5) change this value according to your character y position + 1
        {
            velocity.y += jump;
        }
    }


    private void OnEnable()
    {
        MoveController = playerControls.actions["Move"];
        JumpInput = playerControls.actions["Jump"];

        JumpInput.started += Jump;
        JumpInput.Enable();
    }

    private void OnDisable()
    {
        JumpInput.Disable();
    }


}
