using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHookControls : MonoBehaviour
{
    [Header("Player Inputs and Controller")]
    public CharacterController cc;
    public PlayerInput PlayerControls;
    public GameObject Camera;

    private InputAction HookInput;
    private Vector3 GrapplePoint = Vector3.zero;

    [Header("Grapple Settings")]
    public float GrappleRange = 100f;
    public float GrappleSpeed = 5f;
    public float springConst = 1.2f;

    private bool shouldBeGrappling = false;
    private RaycastHit hit;
    private LayerMask TerrainMask;

    private void OnEnable()
    {
        HookInput = PlayerControls.actions["Fire"];
        HookInput.started += HookStarted;
        HookInput.canceled += HookEnded;
        TerrainMask = LayerMask.GetMask("Terrain");
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
        //print("Started Hookin");
        Vector3 camForward = Camera.transform.forward;
        if (Physics.Raycast(Camera.transform.position, camForward, out hit, GrappleRange, TerrainMask))
        {
            GrapplePoint = hit.point;
            shouldBeGrappling = true;
        }
    }
    private void HookEnded(InputAction.CallbackContext obj)
    {
        EndHook();
    }

    private void Grapple()
    {
        if (GrapplePoint != Vector3.zero)
        {
            Vector3 diff = GrapplePoint - transform.position;
            float distance = Vector3.Distance(GrapplePoint,transform.position);
            cc.Move(diff * Time.deltaTime * (springConst * distance));
        }
    }

    private void EndHook()
    {
        GrapplePoint = Vector3.zero;
        shouldBeGrappling = false;
    }

}
