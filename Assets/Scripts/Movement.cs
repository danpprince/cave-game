using UnityEngine;
using UnityEngine.InputSystem;


public class Movement : MonoBehaviour
{
    public CharacterController cc;
    public float MoveSpeed = 5f;
    public PlayerInput playerControls;
    private InputAction MoveController;
    private Vector2 moveDirection = Vector2.zero;
    private Vector3 velocity;
    public float Speed = 2f;
    public float jump = 10f;
    public float Gravity = -9.8f;


    // Update is called once per frame
    void Update()
    {
        moveDirection = MoveController.ReadValue<Vector2>();
            // for jump
            if (Input.GetKeyUp(KeyCode.Space) && transform.position.y < -0.51f)
            // (-0.5) change this value according to your character y position + 1
            {
                velocity.y = jump;
            }
            else
            {
                velocity.y += Gravity * Time.deltaTime;
            }
            cc.Move(velocity * Time.deltaTime);
        
    }

    private void FixedUpdate()
    {
        Vector3 Move = transform.right * moveDirection.x + transform.forward * moveDirection.y;
        cc.Move(Move * MoveSpeed * Time.deltaTime);
        

    }

    private void OnEnable()
    {
        MoveController = playerControls.actions["Move"];
    }


}
