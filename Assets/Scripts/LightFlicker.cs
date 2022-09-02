using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{

    private float TorchIntensity = 1f;
    private float FlickerDepth = 0.25f;

    void Update()
    {
        this.GetComponent<Light>().range = ((3 * TorchIntensity) + (Mathf.Sin(Time.time) * FlickerDepth));   
    }
}
