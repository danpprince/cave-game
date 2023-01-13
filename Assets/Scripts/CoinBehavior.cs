using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBehavior : MonoBehaviour
{
    public float rotationSpeedDegreesPerSecond;
    public string playerTag;
    private float currentRotationDegrees = 0;

    void Update()
    {
        currentRotationDegrees += rotationSpeedDegreesPerSecond * Time.deltaTime;
        
        if (currentRotationDegrees > 360f)
        {
            currentRotationDegrees -= 360f;
        }

        transform.rotation = Quaternion.Euler(0, currentRotationDegrees, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            GameManager.IncrementNumCoinsCollected();
            Destroy(gameObject);
        }
    }
}
