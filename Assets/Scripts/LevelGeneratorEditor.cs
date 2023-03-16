using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(GenerateLevel))]
public class LevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GenerateLevel generator = (GenerateLevel)target;

        if (GUILayout.Button("Generate Floor"))
        {
            generator.GenerateFloor();
        }
    }
}
