using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    private Light lightComponent;
    private float initialIntensity;
    public float speed = 1.0f, amount = 3.0f;
    private float noiseX, noiseY;

    void Start()
    {
        lightComponent = GetComponent<Light>();
        initialIntensity = lightComponent.intensity;
        noiseX = 0;
        noiseY = Random.value;
    }

    void Update()
    {
        noiseX += speed * Time.deltaTime;
        lightComponent.intensity = 
            (float)(
                initialIntensity + amount * (Mathf.PerlinNoise(noiseX, noiseY) - 0.5)
            );
    }
}
