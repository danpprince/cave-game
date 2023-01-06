using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MarchingCubesTunnelGenerator))]
public class MarchingCubesTunnelGeneratorEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        MarchingCubesTunnelGenerator generator = (MarchingCubesTunnelGenerator)target;

        if (GUILayout.Button("Generate Terrain"))
        {
            generator.ClearTerrainObject();
            generator.PopulateVoxels();
            generator.GenerateMesh();
        }
    }
}
