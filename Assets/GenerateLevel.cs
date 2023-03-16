using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    [Header("Generator Components")]
    public TunnelPathGenerator pathGenerator;
    public MarchingCubesTunnelGenerator terrainGenerator;

    [Header("NavMesh Components")]
    public NavMeshManagerBehavior navMeshManagerBehavior;



    public void GenerateFloor()
    {
        pathGenerator.GeneratePaths();

        terrainGenerator.ClearTerrainObject();
        terrainGenerator.PopulateVoxels();
        terrainGenerator.GenerateMesh();

        navMeshManagerBehavior.Bake();
        navMeshManagerBehavior.NavMesh2Triangles();
        navMeshManagerBehavior.LinkBuilder(navMeshManagerBehavior.jumpRadius);


        


    }
}
