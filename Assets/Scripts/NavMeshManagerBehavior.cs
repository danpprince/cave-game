using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;


public class NavMeshManagerBehavior : MonoBehaviour
{
    
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
        // Bake the terrain
        Bake();
        
        
        // Run the NavMeshTriangulation
        NavMesh2Triangles();
        

        // Run the LinkBuilder
        LinkBuilder(linkSearchLength);


        // Cleanup unlinked vertices
        //DestroyUnlinkedVertices();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Bake()
    {
        // Find the gameObject with the name "Terrain"
        GameObject terrain = GameObject.Find("Terrain");

        // Get the NavMeshSurface component
        NavMeshSurface surface = terrain.GetComponent<NavMeshSurface>();

        // Clear the old NavMesh
        NavMesh.RemoveAllNavMeshData();

        // Bake the new NavMesh
        surface.BuildNavMesh();


        //// Find the layer mask by name
        //int layerMask = LayerMask.GetMask(layerName);

        //// setup world bounds for the bake
        //var worldBounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        //// Fetch the geometry mesh collider
        //var geometry = NavMeshCollectGeometry.RenderMeshes;

        //// Define the list of NavMeshBuildMarkup
        //var markups = new List<NavMeshBuildMarkup>();

        //// Define the list of NavMeshBuildSource in results
        //var results = new List<NavMeshBuildSource>();

        //// Get NavMeshBuildSettings from the agentTypeID
        //var settings = NavMesh.GetSettingsByID(agentTypeID);

        //// Collect Sources from the layer mask
        //NavMeshBuilder.CollectSources(worldBounds, layerMask, geometry, 0, markups, results);

        //// Remove old navMeshData
        //NavMesh.RemoveAllNavMeshData();

        //// Build the new NavMesh
        //NavMeshBuilder.BuildNavMeshData(settings, results, worldBounds, Vector3.zero, Quaternion.identity);
    }

    // Place OffMeshLinks on the NavMesh
    public void LinkBuilder(float searchRadius)
    {
        // Find the instances of vertices in the scene
        GameObject[] vertices = GameObject.FindGameObjectsWithTag(vertexPrefab.tag);

        navMeshVertices.AddRange(vertices);

        Debug.Log("NavMeshVertices: " + navMeshVertices.Count);

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

        Debug.Log("OffMeshLinks: " + navMeshLinks.Count);

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
        
        Debug.Log("NavMesh triangles: " + navMeshTriangleCount);


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

    // Destroy any vertices that do not have a link
    public void DestroyUnlinkedVertices()
    {
        // Find the instances of vertices in the scene
        GameObject[] vertices = GameObject.FindGameObjectsWithTag(vertexPrefab.tag);

        // Update the list of vertices
        navMeshVertices.AddRange(vertices);

        // For each vertex in the list of vertices
        foreach (GameObject vertex in navMeshVertices)
        {
            // If the vertex does not have a link
            if (vertex.GetComponent<OffMeshLink>() == null)
            {
                // Destroy the vertex
                Destroy(vertex);
            }
        }
    }
}
