using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TorchControls : MonoBehaviour
{
    [Header("Player Inputs and Controller")]
    public PlayerInput playerControls;
    public GameObject camera;


    [Header("Torch Controls")]
    public GameObject Torch;
    public GameObject TorchSpawnPoint;
    public float torchUpwardsForce = 5.5f, torchForwardForce = 8f;
    public float TorchForceMultiplier = 2f;
    public GameObject HeldTorch;

    private bool isTorchButtonHeldDown = false;
    private Vector3 DefaultTorchPosition;
    private InputAction TossInput;
    private float DefaultForwardsForce;
    private float torchDistanceMoved = 0;
    private void OnEnable()
    {
        DefaultForwardsForce = torchForwardForce;
        DefaultTorchPosition = HeldTorch.transform.localPosition;
        TossInput = playerControls.actions["Toss"];
        TossInput.Enable();
        TossInput.started += ChargingTorch;
        TossInput.canceled += TossTorch;
    }

    private void OnDisable()
    {
        TossInput.Disable();
    }

    void Update()
    {
        WhileChargingTorchThrow();
    }

    private void ChargingTorch(InputAction.CallbackContext obj)
    {
        isTorchButtonHeldDown = true;
    }

    private void WhileChargingTorchThrow()
    {
        float maxMoveDistance = 1f;
        //print($"distanceMoved {torchDistanceMoved}");
        if (isTorchButtonHeldDown)
        {
            if (torchDistanceMoved < maxMoveDistance)
            {
                float moveDelta = 1f * Time.deltaTime;
                HeldTorch.transform.Translate(0, -moveDelta, 0, Space.World);
                torchDistanceMoved += moveDelta;
            }
        }
        else
        {
            HeldTorch.transform.localPosition = DefaultTorchPosition;
            torchDistanceMoved = 0;
        }
    }

    private void TossTorch(InputAction.CallbackContext obj)
    {
        //Direction Correction
        Vector3 forceDirection = camera.transform.forward;
        RaycastHit hit;

        if (Physics.Raycast(camera.transform.position, forceDirection, out hit, 500f))
        {
            forceDirection = (hit.point - TorchSpawnPoint.transform.position).normalized;
        }

        float heldDuration = Mathf.Min((float)obj.duration, 2.0f);

        torchForwardForce += torchForwardForce * heldDuration * TorchForceMultiplier;
        GameObject TorchInstance = Instantiate(Torch, TorchSpawnPoint.transform.position, camera.transform.rotation);
        Rigidbody torch_rb = TorchInstance.GetComponent<Rigidbody>();
        torch_rb.velocity = gameObject.GetComponent<Rigidbody>().velocity;
        Vector3 ForceToAdd = forceDirection * torchForwardForce + TorchSpawnPoint.transform.up * torchUpwardsForce;
        torch_rb.AddForce(ForceToAdd, ForceMode.Impulse);
        torch_rb.AddTorque(transform.right * Random.Range(5, 20));
        torch_rb.AddTorque(transform.up * Random.Range(-5, 5));
        torchForwardForce = DefaultForwardsForce;
        isTorchButtonHeldDown = false;
    }
}
