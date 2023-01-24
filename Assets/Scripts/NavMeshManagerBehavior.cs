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

    private List<OffMeshLink> navMeshLinks = new List<OffMeshLink>();

    // Start is called before the first frame update
    void Start()
    {
        // Run the NavMeshTriangulation
        NavMesh2Triangles();



        // Run the LinkBuilder
        LinkBuilder(linkSearchLength);

        Debug.Log("Count of NavMeshVertices: " + navMeshVertices.Count);

        Debug.Log("Count of OffMeshLinks: " + navMeshLinks.Count);


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

        float agentHeight = agentPrefab.GetComponent<CapsuleCollider>().height;
        int agentType = agentPrefab.GetComponent<NavMeshAgent>().agentTypeID;

        // find the NavMeshBuildSettings
        NavMeshBuildSettings settings = NavMesh.GetSettingsByID(agentType);

        // Iterate through the vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            // Iterate through the vertices again
            for (int j = 0; j < vertices.Length; j++)
            {
                // If the first vertex and the second are not the same
                if (vertices[i] != vertices[j])
                {
                    // direction of the possible link
                    Vector3 dir = vertices[j].transform.position - vertices[i].transform.position;
                    float yDiff = Mathf.Abs(dir.y);

                    // If the vertices are within a radius
                    if (dir.magnitude <= searchRadius)
                    {
                        // If the vertical difference between the vertices is less than 1.2 * agent height & greater than agent step height
                        if (yDiff <= 1.2f * agentHeight && yDiff > settings.agentClimb)
                        {

                            // if there is not already a link between these two vertices
                            if (!LinkExists(vertices[i], vertices[j]))
                            {
                                
                                // Create a link between the vertices
                                OffMeshLink link = vertices[i].AddComponent<OffMeshLink>();

                                // Set the start position of the offMeshLink to the vertexPrefab with the larger Y value
                                if (vertices[i].transform.position.y > vertices[j].transform.position.y)
                                {
                                    link.startTransform = vertices[i].transform;
                                    link.endTransform = vertices[j].transform;
                                }
                                else
                                {
                                    link.startTransform = vertices[j].transform;
                                    link.endTransform = vertices[i].transform;
                                }

                                // Debug draw a cyan line between the vertices
                                Debug.DrawLine(vertices[i].transform.position, vertices[j].transform.position, Color.cyan, 1000f);

                                link.costOverride = 1.0f;
                                link.activated = true;
                                link.biDirectional = true;
                                link.area = 2; // 2 is the area for jumping



                                // Add the link to the list of links
                                navMeshLinks.Add(link);
                            }

                        }
                    }
                }
            }
        }
    }

    // Check if a link already exists between two vertices
    public bool LinkExists(GameObject vertex1, GameObject vertex2)
    {
        // Iterate through the links
        for (int i = 0; i < navMeshLinks.Count; i++)
        {
            // If the Link goes from vertex1 to vertex2
            if (navMeshLinks[i].startTransform == vertex1.transform && navMeshLinks[i].endTransform == vertex2.transform)
            {
                // Return true
                return true;
            }
            // If the Link goes from vertex2 to vertex1
            else if (navMeshLinks[i].startTransform == vertex2.transform && navMeshLinks[i].endTransform == vertex1.transform)
            {
                // Return true
                return true;
            }
        }

        // Return false
        return false;
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
