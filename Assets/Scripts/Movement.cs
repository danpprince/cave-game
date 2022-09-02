using UnityEngine;
using UnityEngine.InputSystem;


public class Movement : MonoBehaviour
{
    public CharacterController cc;
    public float MoveSpeed = 5f;
    public PlayerInput playerControls;
    private InputAction MoveController;
    private Vector2 moveDirection = Vector2.zero;




    // Start is called before the first frame update
    void Start()
    {
        MoveController = playerControls.actions["Move"];
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = MoveController.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector3 Move = transform.right * moveDirection.x + transform.forward * moveDirection.y;
        cc.Move(Move * MoveSpeed * Time.deltaTime);

    }

  

}
