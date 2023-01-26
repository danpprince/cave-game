using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;


public class NavMeshManagerBehavior : MonoBehaviour
{

    [SerializeField]
    private bool enableDrawTriangles = false;

    [SerializeField]
    private GameObject agentPrefab;

    [SerializeField]
    private GameObject vertexPrefab;

    [SerializeField]
    private float jumpRadius;

    private List<Vector3> navMeshEdgeCorners = new List<Vector3>();

    private List<Vector3> navMeshEdges = new List<Vector3>();

    private List<GameObject> navMeshVertices = new List<GameObject>();

    private List<NavMeshLinkData> navMeshLinks = new List<NavMeshLinkData>();

    // Start is called before the first frame update
    void Start()
    {
        // Bake the terrain
        Bake();


        // Run the NavMeshTriangulation
        NavMesh2Triangles();


        // Run the LinkBuilder
        LinkBuilder(jumpRadius);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Bake()
    {
        // Find the gameObject with the name "GeneratedTerrain"
        GameObject terrain = GameObject.Find("GeneratedTerrain");

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
    public void LinkBuilder(float jumpRadius)
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
                    Vector3 dir = vertices[i].transform.position - vertices[j].transform.position;
                    float yDiff = dir.y;


                    // If the vertices are within a radius
                    if (dir.magnitude <= jumpRadius)
                    {
                        // If the vertical difference between the vertices is greater than agent step height
                        if (Mathf.Abs(yDiff) > settings.agentClimb)
                        {

                            // if there is not already a link between these two points
                            if (!LinkPositionExists(vertices[i].transform.position, vertices[j].transform.position))
                            {

                                // Create a NavMeshLinkData between the vertices
                                NavMeshLinkData linkData = new NavMeshLinkData();

                                // Decide what kind of agent type can use this link
                                linkData.agentTypeID = agentType;

                                // Set the width of the link to 3 * the agent radius
                                linkData.width = settings.agentRadius * 3f;

                                //  Set the start and end positions of the link by finding the position with a larger y value
                                if (vertices[i].transform.position.y > vertices[j].transform.position.y)
                                {
                                    linkData.startPosition = vertices[i].transform.position;
                                    linkData.endPosition = vertices[j].transform.position;


                                }
                                else
                                {
                                    linkData.startPosition = vertices[j].transform.position;
                                    linkData.endPosition = vertices[i].transform.position;


                                }

                                Vector3 midPoint = (linkData.startPosition + linkData.endPosition) / 2;

                                // Debug draw a yellow line from the startPosition to the midpoint
                                Debug.DrawLine(linkData.startPosition, midPoint, Color.yellow, 1000f);

                                // Debug draw a white line from the endPosition to the midpoint
                                Debug.DrawLine(linkData.endPosition, midPoint, Color.white, 1000f);

                                // Debug draw a cyan line between the vertices
                                //Debug.DrawLine(vertices[i].transform.position, vertices[j].transform.position, Color.cyan, 1000f);

                                // make the link bi-directional
                                linkData.bidirectional = true;

                                // adjust the cost of the link for pathfinding
                                linkData.costModifier = 2f;

                                // Add the linkData to the NavMesh
                                NavMesh.AddLink(linkData);


                                // Add the linkData to the list of links
                                navMeshLinks.Add(linkData);


                            }

                        }
                    }
                }
            }
        }

        Debug.Log("OffMeshLinks: " + navMeshLinks.Count);

    }

    // Check if a link already exists between two positions
    public bool LinkPositionExists(Vector3 pos1, Vector3 pos2)
    {
        // Iterate through the list of links
        for (int i = 0; i < navMeshLinks.Count; i++)
        {
            // If the start and end positions of the link are the same as the positions passed in
            if (navMeshLinks[i].startPosition == pos1 && navMeshLinks[i].endPosition == pos2)
            {
                return true;
            }
            // If the start and end positions of the link are the same as the positions passed in
            else if (navMeshLinks[i].startPosition == pos2 && navMeshLinks[i].endPosition == pos1)
            {
                return true;
            }
        }

        return false;
    }

    // Check if a link already exists between two vertices
    public bool LinkExists(GameObject vertex1, GameObject vertex2)
    {
        // Iterate through the links
        for (int i = 0; i < navMeshLinks.Count; i++)
        {
            // If the Link goes from vertex1 to vertex2
            if (navMeshLinks[i].startPosition == vertex1.transform.position && navMeshLinks[i].endPosition == vertex2.transform.position)
            {
                // Return true
                return true;
            }
            // If the Link goes from vertex2 to vertex1
            else if (navMeshLinks[i].startPosition == vertex2.transform.position && navMeshLinks[i].endPosition == vertex1.transform.position)
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

}
