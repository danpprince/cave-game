using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hitByWhip : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "whip")
        {
            print("Whip has collided");
            Rigidbody rb = this.GetComponent<Rigidbody>();
            rb.AddExplosionForce(100f, collision.gameObject.transform.position,10f,1.0f);
        }
    }
}

