using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshManagerBehavior : MonoBehaviour
{
    //private Mesh mesh;

    [SerializeField]
    private bool enableDrawTriangles = false;

    [SerializeField]
    private GameObject agentPrefab;

    [SerializeField]
    private GameObject vertexPrefab;

    [SerializeField]
    private float linkSearchLength;

    private List<Vector3> navMeshEdgeCorners = new List<Vector3>();

    private List<Vector3> navMeshEdges = new List<Vector3>();

    private List<GameObject> navMeshVertices = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // Run the NavMeshTriangulation
        NavMesh2Triangles();
        
        
        
        // Run the LinkBuilder
        LinkBuilder(linkSearchLength);

        Debug.Log("Count of NavMeshVertices: " + navMeshVertices.Count);
    }

    // Update is called once per frame
    void Update()
    {

    }


    // Check vertices near each other (within a radius)
    // that do not have a NavMesh hit at the midpoint of their centers   
    public void LinkBuilder(float searchRadius)
    {
        // Find the instances of vertices in the scene
        GameObject[] vertices = GameObject.FindGameObjectsWithTag(vertexPrefab.tag);

        navMeshVertices.AddRange(vertices);

        // Iterate through the vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            // Iterate through the vertices again
            for (int j = 0; j < vertices.Length; j++)
            {
                // If the first vertex and the second are not the same
                if (vertices[i] != vertices[j])
                {
                    // If the vertices are within a radius
                    if (Vector3.Distance(vertices[i].transform.position, vertices[j].transform.position) <= searchRadius)
                    {
                        // If the midpoint of the vertices does not have a NavMesh hit
                        if (!NavMesh.SamplePosition((vertices[i].transform.position + vertices[j].transform.position) / 2, out NavMeshHit hit, 1, NavMesh.AllAreas))
                        {
                            // create an OffMeshLink object between the vertices
                            OffMeshLink offMeshLink = vertices[i].AddComponent<OffMeshLink>();

                            // Set the start position of the offMeshLink to the vertexPrefab with the larger Y value
                            if (vertices[i].transform.position.y > vertices[j].transform.position.y)
                            {
                                offMeshLink.startTransform = vertices[i].transform;
                                offMeshLink.endTransform = vertices[j].transform;
                            }
                            else
                            {
                                offMeshLink.startTransform = vertices[j].transform;
                                offMeshLink.endTransform = vertices[i].transform;
                            }

                            // set the OffMeshLink biDirectional to true
                            offMeshLink.biDirectional = true;

                        }
                    }
                }
            }
        }

    }
    // A function to draw the triangles of the NavMesh
    public void NavMesh2Triangles()
    {
        // Get the triangles from the NavMesh
        NavMeshTriangulation navMeshTriangles = NavMesh.CalculateTriangulation();

        // Count the triangles on the NavMesh
        int navMeshTriangleCount = navMeshTriangles.indices.Length / 3;
        Debug.Log("NavMesh triangles counted: " + navMeshTriangleCount);


        // Go through each triangle on the NavMesh
        for (int i = 0; i < navMeshTriangles.indices.Length; i += 3)
        {
            if (enableDrawTriangles)
            {
                // Draw the triangles on the NavMesh
                Debug.DrawLine(navMeshTriangles.vertices[navMeshTriangles.indices[i]], navMeshTriangles.vertices[navMeshTriangles.indices[i + 1]], Color.red, 1000f);
                Debug.DrawLine(navMeshTriangles.vertices[navMeshTriangles.indices[i + 1]], navMeshTriangles.vertices[navMeshTriangles.indices[i + 2]], Color.red, 1000f);
                Debug.DrawLine(navMeshTriangles.vertices[navMeshTriangles.indices[i + 2]], navMeshTriangles.vertices[navMeshTriangles.indices[i]], Color.red, 1000f);
            }

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
                    // Add the vertex of the triangle to the list of navMeshEdgeCorners
                    navMeshEdgeCorners.Add(navMeshTriangles.vertices[navMeshTriangles.indices[i + j]]);

                    // Spawn a vertexPrefab at the corner of the triangle on the NavMesh edge as a child of this object
                    GameObject vertex = Instantiate(vertexPrefab, navMeshTriangles.vertices[navMeshTriangles.indices[i + j]], Quaternion.identity, this.transform);

                }

            }
        }
    }

}
