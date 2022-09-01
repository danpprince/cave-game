using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class LookAround : MonoBehaviour
{
    public PlayerInput PlayerControls;
    private InputAction Look;
    public float AimSensitivity;
    private Vector2 MousePosition;
    public Transform playerBody;
    private float X_Rotation = 0;


    // Start is called before the first frame update
    void Start()
    {
       Look = PlayerControls.actions["Look"];
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        MousePosition = Look.ReadValue<Vector2>();
        
        float Look_X = MousePosition.x * AimSensitivity * Time.deltaTime;
        float Look_Y = MousePosition.y * AimSensitivity * Time.deltaTime;

        //Rotates entire Player object.
        playerBody.Rotate(Vector3.up * Look_X);
        

        X_Rotation -= Look_Y;
        X_Rotation = Mathf.Clamp(X_Rotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(X_Rotation, 0f, 0f);
        
        




    }
}
