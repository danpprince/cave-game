using UnityEngine;
using UnityEngine.InputSystem;


public class Movement : MonoBehaviour
{
    public Rigidbody rb;
    public float MoveSpeed = 5f;
    public PlayerInput playerControls;
    private InputAction move;
    private Vector2 moveDirection = Vector2.zero;




    // Start is called before the first frame update
    void Start()
    {
        move = playerControls.actions["Move"];
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = move.ReadValue<Vector2>();

        print(moveDirection);
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector3(moveDirection.y * MoveSpeed, 0,  moveDirection.x * MoveSpeed);
    }

    private void OnEnable()
    {
        //playerControls.Enable();
    }

    private void OnDisable()
    {
        //playerControls.Disable();
    }


}
