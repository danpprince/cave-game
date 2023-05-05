using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevelOnStart : MonoBehaviour
{
    public TunnelPathGenerator tunnelPathGenerator;
    public MarchingCubesTunnelGenerator marchingCubesTunnelGenerator;

    void Start()
    {
        print("Generating tunnel paths");
        tunnelPathGenerator.GeneratePaths();

        print("Creating mesh with marching cubes");
        marchingCubesTunnelGenerator.ClearTerrainObject();
        marchingCubesTunnelGenerator.PopulateVoxels();
        marchingCubesTunnelGenerator.GenerateMesh();

        GameManager.CountCoinsInLevel();
    }
}
