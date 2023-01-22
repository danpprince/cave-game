using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BuildNonNavSurface : MonoBehaviour
{
    private Mesh mesh;

    [SerializeField]
    private GameObject agentPrefab;

    // declare list of Vector3 for navMeshEdgeCorners
    private List<Vector3> navMeshEdgeCorners = new List<Vector3>();

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

        // Go through each triangle on the NavMesh
        for (int i = 0; i < navMeshTriangles.indices.Length; i += 3)
        {
            // Array of Vector3 for the nearest NavMesh Edge hits to each vertex of the triangle
            NavMeshHit[] nearestEdgeHits = new NavMeshHit[3];

            // Find nearestEdgeHit to the NavMeshTriangles vertices
            NavMesh.FindClosestEdge(navMeshTriangles.vertices[navMeshTriangles.indices[i]], out nearestEdgeHits[0], NavMesh.AllAreas);
            NavMesh.FindClosestEdge(navMeshTriangles.vertices[navMeshTriangles.indices[i + 1]], out nearestEdgeHits[1], NavMesh.AllAreas);
            NavMesh.FindClosestEdge(navMeshTriangles.vertices[navMeshTriangles.indices[i + 2]], out nearestEdgeHits[2], NavMesh.AllAreas);

            // Go through each nearestEdgeHit
            for (int j = 0; j < nearestEdgeHits.Length; j++)
            {
                                
                // Check if the distance between the NavMeshTriangle vertices to the nearestEdgeHit is less than 0.1f
                if (Vector3.Distance(navMeshTriangles.vertices[navMeshTriangles.indices[i + j]], nearestEdgeHits[j].position) < 0.1f)
                {
                    // Add the corner of the triangle to the list of navMeshEdgeCorners
                    navMeshEdgeCorners.Add(nearestEdgeHits[j].position);

                    
                }
                
                // Draw all navMeshTriangle edges in red
                Debug.DrawLine(navMeshTriangles.vertices[navMeshTriangles.indices[i]], navMeshTriangles.vertices[navMeshTriangles.indices[i + 1]], Color.red, 1000f);
                Debug.DrawLine(navMeshTriangles.vertices[navMeshTriangles.indices[i + 1]], navMeshTriangles.vertices[navMeshTriangles.indices[i + 2]], Color.red, 1000f);
                Debug.DrawLine(navMeshTriangles.vertices[navMeshTriangles.indices[i + 2]], navMeshTriangles.vertices[navMeshTriangles.indices[i]], Color.red, 1000f);

                // Find all navMeshTriangle edges that are between navMeshEdgeCorners
                
            }






        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
