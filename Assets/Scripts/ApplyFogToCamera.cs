// From https://en.wikibooks.org/wiki/Cg_Programming/Unity/Minimal_Image_Effect

using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]

public class ApplyFogToCamera : MonoBehaviour
{

    public Material material;

    void Start()
    {
        if (null == material || null == material.shader ||
           !material.shader.isSupported)
        {
            enabled = false;
            return;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, material);
    }
}