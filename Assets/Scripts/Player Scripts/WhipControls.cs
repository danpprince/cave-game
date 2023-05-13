using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhipControls : MonoBehaviour
{
    [Header("Player Inputs and Controller")]
    public PlayerInput playerControls;
    public GameObject camera;


    [Header("Whip Controls")]
    public float whipCoolDownTime = 1f;
    public float whipRange = 5f;
    public bool didWhipHit = false;
    public float whipAnimationSpeed = 1f;
    public GameObject whipBase;
    public float whipArchHeight = 0.5f;
    public float whipHitColliderRadius = 0.5f;
    public GameObject whipHitCollider;

    private bool canFireWhip = true;
    private InputAction WhipInput;
    private Vector3 whipHitPosition;
    private RaycastHit hit;
    private float hitDistance;
    private LineRenderer lineRenderer;
    private bool shouldAnimateWhip = false;
    private Vector3 currentWhipTipPosition;

    private void OnEnable()
    {
        WhipInput = playerControls.actions["Fire"];
        WhipInput.started += useWhip;
    }

    void FixedUpdate()
    {
        findWhipPoint();
        if (shouldAnimateWhip)
        {
            animateWhip();
        } 
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = didWhipHit ? Color.red : Color.green;
        Gizmos.DrawSphere(whipHitPosition, 0.1f);
        Gizmos.DrawSphere(currentWhipTipPosition, 0.1f);
    }

    IEnumerator WhipCoolDownTimer()
    {
        yield return new WaitForSeconds(whipCoolDownTime);
        canFireWhip = true;
    }
    IEnumerator WhipAnimationTimer()
    {
        yield return new WaitForSeconds(1 / whipAnimationSpeed);
        Destroy(lineRenderer);
        shouldAnimateWhip = false;
    }

    private void findWhipPoint()
    {

        Vector3 camForward = camera.transform.forward;
        if (Physics.Raycast(camera.transform.position, camForward, out hit, whipRange))
        {
            Debug.DrawRay(camera.transform.position, transform.TransformDirection(Vector3.forward) * whipRange, Color.red);
            hitDistance = hit.distance;
            whipHitPosition = hit.point;
            didWhipHit = true;
        }
        else
        {
            Debug.DrawRay(camera.transform.position, transform.TransformDirection(Vector3.forward) * whipRange, Color.green);
            whipHitPosition = camera.transform.position + camera.transform.forward * whipRange;
            didWhipHit = false;
        }
    }

    private void useWhip(InputAction.CallbackContext obj)
    {
        if (canFireWhip)
        {
            string message = didWhipHit ? "Hit " + hit.collider.name : "Missed";
            Debug.Log(message);
            canFireWhip = false;
            shouldAnimateWhip = true;
            StartCoroutine(WhipCoolDownTimer());
            StartCoroutine(WhipAnimationTimer());
            if (didWhipHit)
            {
                GameObject hitCollider = Instantiate(whipHitCollider, whipHitPosition, Quaternion.identity);
            }
        }
    }

    private void animateWhip()
    {
        Vector3[] whipPoints = new Vector3[3];
        Vector3 basePosition = whipBase.transform.position;
        Vector3 whipHitPositionLocal = transform.InverseTransformPoint(whipHitPosition);

        Vector3 peak = (whipBase.transform.position); 
        peak = new Vector3(peak.x, peak.y + whipArchHeight, peak.z);

        whipPoints[0] = whipBase.transform.position;
        whipPoints[1] = peak;
        whipPoints[2] = whipHitPosition;

        BezierCurve curve = new BezierCurve(whipPoints);
        Vector3[] curveSegments = curve.GetSegments(10);
        //Vector3[] currentSegments = 
    }

}
