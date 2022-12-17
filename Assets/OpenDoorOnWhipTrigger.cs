using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoorOnWhipTrigger : MonoBehaviour
{
    private bool isLeverDown = true;
    public Animator doorAnimator, leverAnimator;

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
                leverAnimator.Play("MoveDown");
            } else
            {
                print("Starting open door animation");
                doorAnimator.Play("Open");
                leverAnimator.Play("MoveUp");
            }
        }
    }
}
