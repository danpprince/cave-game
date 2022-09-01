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


    // Start is called before the first frame update
    void Start()
    {
       Look = PlayerControls.actions["Look"];
        
    }

    // Update is called once per frame
    void Update()
    {
        MousePosition = Look.ReadValue<Vector2>();
        float Mouse_X = MousePosition.x * AimSensitivity;
        float Mouse_Y = MousePosition.y * AimSensitivity;




    }
}
