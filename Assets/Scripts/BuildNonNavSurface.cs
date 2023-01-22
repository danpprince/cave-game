using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BuildNonNavSurface : MonoBehaviour
{
    private Mesh mesh;

    [SerializeField]
    private GameObject agentPrefab;

    // Start is called before the first frame update
    void Start()
    {
        // Find the mesh from the "Terrain" object in the scene
        mesh = GameObject.Find("Terrain").GetComponent<MeshFilter>().mesh;

        // Find the agentTypeID for the agentPrefab
        int agentTypeID = agentPrefab.GetComponent<NavMeshAgent>().agentTypeID;

        // Find the NavMeshBuildSettings for the agentTypeID
        NavMeshBuildSettings settings = NavMesh.GetSettingsByID(agentTypeID);

        // Count each triangle on the mesh
        int triangleCount = mesh.triangles.Length / 3;
        Debug.Log("Terrain triangles counted: " + triangleCount);

        // Get the triangles from the NavMesh
        NavMeshTriangulation navMeshTriangles = NavMesh.CalculateTriangulation();

        // Count the triangles on the NavMesh
        int navMeshTriangleCount = navMeshTriangles.indices.Length / 3;
        Debug.Log("NavMesh triangles counted: " + navMeshTriangleCount);

        // Draw the edges of the triangles on the NavMesh
        for (int i = 0; i < navMeshTriangles.indices.Length; i += 3)
        {
            Debug.DrawLine(navMeshTriangles.vertices[navMeshTriangles.indices[i]], navMeshTriangles.vertices[navMeshTriangles.indices[i + 1]], Color.red, 1000f);
            Debug.DrawLine(navMeshTriangles.vertices[navMeshTriangles.indices[i + 1]], navMeshTriangles.vertices[navMeshTriangles.indices[i + 2]], Color.red, 1000f);
            Debug.DrawLine(navMeshTriangles.vertices[navMeshTriangles.indices[i + 2]], navMeshTriangles.vertices[navMeshTriangles.indices[i]], Color.red, 1000f);
        }

        // 

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
