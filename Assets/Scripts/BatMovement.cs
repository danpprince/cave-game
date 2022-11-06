using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatMovement : MonoBehaviour
{
    public float forwardTestDistance;
    public float testRadius;
    public float movementSpeed;
    public float rotationSpeed;
    public float collisionRotationSpeed;
    public float rotationRandomAmount;

    // Update is called once per frame
    void Update()
    {
        Vector3 forwardTestPoint = transform.position + transform.forward * forwardTestDistance;
        float rotationToApply = rotationSpeed + Random.Range(-rotationRandomAmount, rotationRandomAmount);
        //if (!Physics.CheckSphere(forwardTestPoint, testRadius))
        if (!Physics.Raycast(transform.position, transform.forward, forwardTestDistance))
        {
            transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime);
        } else
        {
            rotationToApply = collisionRotationSpeed;
        }

        transform.Rotate(0, rotationToApply * Time.deltaTime, 0);
    }
}
