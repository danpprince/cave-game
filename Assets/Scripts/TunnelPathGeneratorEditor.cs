using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TunnelPathGenerator))]
public class TunnelPathGeneratorEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        TunnelPathGenerator generator = (TunnelPathGenerator)target;

        if (GUILayout.Button("Generate Paths"))
        {
            generator.GeneratePaths();
        }
    }
}
