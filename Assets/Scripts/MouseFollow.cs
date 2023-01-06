using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class MouseFollow : MonoBehaviour
{

    public float forcestrength = 10f;

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseSPACE = new Vector3 (Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 50f);
        Vector3 worldSpace = Camera.main.ScreenToWorldPoint(mouseSPACE);
        transform.position = Vector3.MoveTowards(this.transform.position,worldSpace, 10f);

    }
}
