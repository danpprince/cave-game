using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoorOnWhipTrigger : MonoBehaviour
{
    private bool isLeverDown = true;
    public Animator doorAnimator;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collider)
    {
        print("Lever trigger entered by collider " + collider.name);
        if (collider.gameObject.tag == "whip")
        {
            isLeverDown = !isLeverDown;
            if (isLeverDown)
            {
                print("Starting close door animation");
                doorAnimator.Play("Close");
            } else
            {
                print("Starting open door animation");
                doorAnimator.Play("Open");
            }
            // TODO: Animate the lever
        }
    }
}
