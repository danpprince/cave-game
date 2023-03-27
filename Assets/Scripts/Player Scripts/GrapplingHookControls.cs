using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;

public class GrapplingHookControls : MonoBehaviour
{
    [Header("Player Inputs and Controller")]
    private Rigidbody rb;
    public PlayerInput PlayerControls;
    public GameObject Camera;

    private Movement _Movement;
    private InputAction HookInput;
    private Vector3 GrapplePoint = Vector3.zero;

    [Header("Grapple Settings")]
    public float GrappleRange = 100f;
    public float GrappleSpeed = 5f;
    public float springConst = 1.2f;
    public float MinimumGrappleLength = 2f;
    public float SpringTension = 1f;
    public float Damper = 0.25f;


    private bool shouldBeGrappling = false;
    private RaycastHit hit;
    private LayerMask TerrainMask;
    private SpringJoint GrappleRope;

    private void OnEnable()
    {
        HookInput = PlayerControls.actions["Fire"];
        HookInput.started += HookStarted;
        HookInput.canceled += HookEnded;
        TerrainMask = LayerMask.GetMask("Terrain");
        _Movement = gameObject.GetComponent<Movement>();
    }

    public void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (shouldBeGrappling)
        {
            Grapple();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(GrapplePoint, 0.1f);
    }



    private void HookStarted(InputAction.CallbackContext obj)
    {
        Vector3 camForward = Camera.transform.forward;
        if (Physics.Raycast(Camera.transform.position, camForward, out hit, GrappleRange, TerrainMask))
        {
            GrapplePoint = hit.point;
            shouldBeGrappling = true;
            GrappleRope = gameObject.AddComponent<SpringJoint>() as SpringJoint;
            GrappleRope.autoConfigureConnectedAnchor = false;
            GrappleRope.connectedAnchor = hit.point;
            GrappleRope.spring = SpringTension;
            GrappleRope.damper = Damper;
            
        }
    }

    private void Grapple()
    {
        //if (GrapplePoint != Vector3.zero)
        //{
        //    _Movement.ignoreGravity = true;
        //    Vector3 diff = (GrapplePoint - transform.position).normalized;
        //    float distance = Vector3.Distance(GrapplePoint,transform.position);
        //    float displacement = distance - MinimumGrappleLength;
        //    float springForce = displacement * springConst;
        //    float acceleration =  (rb.velocity.magnitude) * distance;
        //}
    }
        
    private void EndHook()
    {
        GrapplePoint = Vector3.zero;
        shouldBeGrappling = false;
        Destroy(GrappleRope);
    }

    private void HookEnded(InputAction.CallbackContext obj)
    {
        EndHook();
    }
}
