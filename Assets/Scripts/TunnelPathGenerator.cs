using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PathCreation;

/// <summary>
/// Generates tunnels recursively consisting of paths and room triggers (not meshes)
/// </summary>
public class TunnelPathGenerator : MonoBehaviour
{
    [Header("Room references")]
    /// <summary>
    /// Center of starting room to use in first call for new room
    /// </summary>
    public Transform startingRoomTransform;

    /// <summary>
    /// Starting endpoint to determine initial direction for first new room
    /// </summary>
    public Transform startingTunnelEndpoint;

    /// <summary>
    /// Room object with trigger collider and tunnel endpoints to use in recursive generation
    /// </summary>
    public GameObject roomPrefab;

    /// <summary>
    /// Tag in "roomPrefab" children that should be used to create tunnels from
    /// </summary>
    public string tunnelEndpointTag;

    [Header("Path generation")]
    /// <summary>
    /// Maximum number depth of recursive calls for creating new rooms
    /// </summary>
    public int maxRecursionDepth;

    /// <summary>
    /// Probability that each tunnel endpoint will be used for a recursive call (between 0.0 and 1.0)
    /// </summary>
    public float recursionProbability;

    /// <summary>
    /// If fewer rooms are generated than this value, all paths will be regenerated.
    /// WARNING: Can cause an infinite loop if too large relative to other parameters
    /// </summary>
    public int minimumRoomCount;

    /// <summary>
    /// Upper or lower limit for how long paths between rooms may be
    /// </summary>
    public float tunnelLengthMin, tunnelLengthMax;

    /// <summary>
    /// Upper limit for how much the midpoints between rooms may be moved in the XZ plane
    /// </summary>
    public float midpointOffsetRange;

    /// <summary>
    /// Maximum angle in degrees for downward slope between rooms
    /// </summary>
    public float tunnelDownwardAngleMaxDegrees;

    [Header("Occupancy")]
    /// <summary>
    /// If true, keeps rooms and paths from being generated on top of each other
    /// based on the occupancy parameters
    /// </summary>
    public bool preventOverlappingPaths;

    /// <summary>
    /// The size of occupancy cells in world resolution. Larger values will reduce computation
    /// and spread rooms out more. Requires preventOverlappingPaths to be true to have any effect.
    /// </summary>
    public float occupancyCellResolution;

    /// <summary>
    /// Keeps track of cells where paths have been generated. Only used if preventOverlappingPaths is true.
    /// </summary>
    private HashSet<Vector3> occupiedCells;

    /// <summary>
    /// Number of times to try to generate a room in an unoccupied location before failing.
    /// Only used if preventOverlappingPaths is true.
    /// </summary>
    public int numOccupancyRetries;

    [Header("Etc")]
    /// <summary>
    /// Coin prefab to generate in "dead end" rooms at the end of paths.
    /// </summary>
    public GameObject coinPrefab;

    /// <summary>
    /// TODO
    /// </summary>
    public GameObject goalPrefab;

    /// <summary>
    /// Number of rooms that have been generated so far. Used to compare against minimumRoomCount.
    /// </summary>
    private int currentRoomCount;

    /// <summary>
    /// Reference to new object that will parent all newly generated paths and rooms
    /// </summary>
    private GameObject generatedParent;

    /// <summary>
    /// TODO
    /// </summary>
    private List<Vector3> tunnelDeadEndPositions;

    /// <summary>
    /// Create tunnel paths with Bezier curves and colliders
    /// </summary>
    public void GeneratePaths()
    {
        int maxNumAttempts = 10;
        for (int generationAttempt = 0; generationAttempt < maxNumAttempts; generationAttempt++)
        {
            tunnelDeadEndPositions = new List<Vector3>();

            GameManager.RestartState();
            occupiedCells = new HashSet<Vector3>();
            generatedParent = new GameObject("GeneratedPaths");
            generatedParent.transform.parent = gameObject.transform;
            Vector3 startingPosition = startingTunnelEndpoint.position;
            Vector3 baseDirection = (startingPosition - startingRoomTransform.position).normalized;

            currentRoomCount = 0;
            GeneratePathsRecursive(startingPosition, baseDirection, 0);

            if (currentRoomCount < minimumRoomCount)
            {
                Debug.Log(
                    $"Current room count {currentRoomCount} less than required minimum {minimumRoomCount} "
                    + $"on attempt {generationAttempt}"
                );
                DestroyImmediate(generatedParent);
                generationAttempt++;
                continue;
            }

            PopulateDeadEnds();

            // Save generated data to generated directory so it does not inflate scene files
            Utils.SaveGameObjectAsPrefab(generatedParent, "GeneratedPaths");
            return;
        }
        Debug.LogError(
            $"Unable to generate {minimumRoomCount} rooms after {maxNumAttempts} attempts, "
            + "adjust generation parameters to create more rooms or decrease the minimum required rooms"
        );
    }

    /// <summary>
    /// Generate one new collider "room" from a source and recursively call for the next rooms
    /// </summary>
    /// <param name="sourcePosition">Starting position where the new path should originate</param>
    /// <param name="baseDirection">General direction the new room should be created in relative to sourceTransform</param>
    /// <param name="depth">, should be 0 initially</param>
    /// <returns>True if at least one child room is created</returns>
    private bool GeneratePathsRecursive(Vector3 sourcePosition, Vector3 baseDirection, int depth)
    {
        if (depth >= maxRecursionDepth)
        {
            return false;
        }

        GameObject destinationRoom = MakeDestinationRoom(sourcePosition, baseDirection);
        if (destinationRoom is null)
        {
            return false;
        }
        (Transform pathDestination, List<Transform> childTransformsToRecurseInto) =
            GetRoomEndpoints(destinationRoom, sourcePosition);
        VertexPath path = MakePathToDestination(sourcePosition, pathDestination.position);
        if (path is null)
        {
            DestroyImmediate(destinationRoom); // TODO: Any other object cleanup here?
            return false;
        }

        if (preventOverlappingPaths)
        {
            MarkCellsAsOccupied(destinationRoom, path);
        }
        currentRoomCount++;

        bool isTunnelDeadEnd = true;
        foreach (Transform childTransform in childTransformsToRecurseInto)
        {
            // Skip recursion randomly or if the current child is the same as the completed tunnel destination
            if (Random.value > recursionProbability || childTransform == pathDestination)
            {
                continue;
            }
            Vector3 childPosition = childTransform.position;
            Vector3 childBaseDirection = (childPosition - destinationRoom.transform.position).normalized;
            isTunnelDeadEnd &= !GeneratePathsRecursive(childPosition, childBaseDirection, depth + 1);
        }

        if (isTunnelDeadEnd)
        {
            tunnelDeadEndPositions.Add(destinationRoom.transform.position);
        }
        return true;
    }

    /// <summary>
    /// TODO
    /// </summary>
    void PopulateDeadEnds()
    {
        // Make a goal object in a random dead end position
        int goalPositionIndex = Random.Range(0, tunnelDeadEndPositions.Count);
        Vector3 goalPosition = tunnelDeadEndPositions[goalPositionIndex];
        InstantiateGoal(goalPosition);
        tunnelDeadEndPositions.RemoveAt(goalPositionIndex);

        // Put coins in the rest
        foreach (Vector3 position in tunnelDeadEndPositions)
        {
            InstantiateCoin(position);
        }
    }

    private void InstantiateGoal(Vector3 position)
    {
        Instantiate(goalPrefab, position, Quaternion.identity, generatedParent.transform);
    }

    private void InstantiateCoin(Vector3 position)
    {
        Instantiate(coinPrefab, position, Quaternion.identity, generatedParent.transform);
    }

    /// <summary>
    /// Create a new GameObject for a destination room
    /// </summary>
    /// <param name="sourcePosition"></param>
    /// <param name="baseDirection"></param>
    /// <returns></returns>
    private GameObject MakeDestinationRoom(Vector3 sourcePosition, Vector3 baseDirection)
    {
        for (int retryIndex = 0; retryIndex < numOccupancyRetries; retryIndex++)
        {
            float tunnelLength = Random.Range(tunnelLengthMin, tunnelLengthMax);
            Vector3 roomPosition = sourcePosition + baseDirection * tunnelLength;
            float downwardAngle = Random.Range(0, tunnelDownwardAngleMaxDegrees);
            float yChange = tunnelLength * Mathf.Tan(downwardAngle * Mathf.Deg2Rad);
            roomPosition.y += Random.Range(-yChange, 0f);

            Quaternion roomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            Vector3 roomPositionQuantized = QuantizeToOccupancyGrid(roomPosition);
            if (preventOverlappingPaths && occupiedCells.Contains(roomPositionQuantized)) {
                Debug.Log($"Room position {roomPositionQuantized} already occupied on retry {retryIndex}");
            }
            else {
                GameObject room = Instantiate(roomPrefab, roomPosition, roomRotation, generatedParent.transform);
                return room;
            }
        }

        return null;
    }

    /// <summary>
    /// Find the tunnel endpoints that are children of a room
    /// </summary>
    /// <param name="room"></param>
    /// <param name="sourcePosition"></param>
    /// <returns>The transform of the tunnel endpoint closest to the source position, and
    /// all tunnel endpoints in the room including the closest one</returns>
    private (Transform, List<Transform>) GetRoomEndpoints(GameObject room, Vector3 sourcePosition)
    {
        // Set the path destination to the closest TunnelEndpoint child in the new room
        Transform closestTransform = null;
        float pathDistance = float.PositiveInfinity;
        List<Transform> allChildTransforms = new List<Transform>();
        for (int childIndex = 0; childIndex < room.transform.childCount; childIndex++)
        {
            GameObject childObject = room.transform.GetChild(childIndex).gameObject;
            if (!childObject.CompareTag(tunnelEndpointTag))
            {
                continue;
            }

            allChildTransforms.Add(childObject.transform);

            if (closestTransform is null)
            {
                closestTransform = childObject.transform;
                pathDistance = Vector3.Distance(closestTransform.position, sourcePosition);
                print($"Closest destination was null, now {closestTransform} with distance {pathDistance}");
                continue;
            }

            float newTunnelDistance = Vector3.Distance(sourcePosition, childObject.transform.position);
            if (newTunnelDistance < pathDistance)
            {
                closestTransform = childObject.transform;
                pathDistance = Vector3.Distance(closestTransform.position, sourcePosition);
                print($"Shorter path found, now destination {closestTransform} with distance {pathDistance}");
                continue;
            }
        }

        return (closestTransform, allChildTransforms);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourcePosition"></param>
    /// <param name="destinationPosition"></param>
    private VertexPath MakePathToDestination(Vector3 sourcePosition, Vector3 destinationPosition)
    {
        bool isPathOccupied = false;
        VertexPath vertexPath = null;
        for (int retryIndex = 0; retryIndex < numOccupancyRetries; retryIndex++)
        {
            // Create the bezier path to the selected destination
            GameObject pathObject = new GameObject("Path");
            pathObject.transform.parent = generatedParent.transform;

            Vector3 midpoint = Vector3.Lerp(sourcePosition, destinationPosition, 0.5f);
            midpoint += new Vector3(
                Random.Range(-midpointOffsetRange, midpointOffsetRange),
                0,
                Random.Range(-midpointOffsetRange, midpointOffsetRange)
            );
            List<Vector3> pathPoints = new List<Vector3> {
                sourcePosition, midpoint, destinationPosition
            };

            BezierPath bezierPath = new BezierPath(pathPoints);
            vertexPath = new VertexPath(bezierPath, pathObject.transform);

            // Check if the path is occupied
            isPathOccupied = false;
            float initialDistance = occupancyCellResolution; // Travel outside the room to check occupancy
            float pathOccupancyStepDistance = occupancyCellResolution / 2;
            for (float distance = initialDistance; distance < vertexPath.length; distance += pathOccupancyStepDistance)
            {
                Vector3 occupancyQueryPoint = vertexPath.GetPointAtDistance(distance, EndOfPathInstruction.Stop);
                Vector3 quantizedQueryPoint = QuantizeToOccupancyGrid(occupancyQueryPoint);
                if (preventOverlappingPaths && occupiedCells.Contains(quantizedQueryPoint))
                {
                    Debug.Log($"Path in cell {quantizedQueryPoint} is occupied on retry {retryIndex}");
                    isPathOccupied = true;
                    break;
                }
            }

            if (isPathOccupied)
            {
                DestroyImmediate(pathObject);
                continue;
            }

            PathCreator tunnelPathCreator = pathObject.AddComponent<PathCreator>();
            tunnelPathCreator.bezierPath = bezierPath;

            // Toggle control mode so automatic tangent points are set correctly
            tunnelPathCreator.bezierPath.ControlPointMode = BezierPath.ControlMode.Aligned;
            tunnelPathCreator.bezierPath.ControlPointMode = BezierPath.ControlMode.Automatic;
            break;
        }

        if (isPathOccupied)
        {
            Debug.Log($"Unable to create path in unoccupied cells");
            return null;
        }

        return vertexPath;
    }

    /// <summary>
    /// Populated occupied cells for a room and a path
    /// </summary>
    /// <param name="room">Room containing a collider to occupy cells with</param>
    /// <param name="path">Path to occupy cells along</param>
    private void MarkCellsAsOccupied(GameObject room, VertexPath path)
    {
        float margin = occupancyCellResolution / 2;
        Vector3 marginVector = Vector3.one * margin;
        Collider collider = room.GetComponent<Collider>();
        Vector3 minPoint = collider.bounds.min - marginVector;
        Vector3 maxPoint = collider.bounds.max + marginVector;

        Debug.Log($"Occupying cells for room with min {minPoint}, max {maxPoint}");

        for (float x = minPoint.x; x <= maxPoint.x; x += occupancyCellResolution)
        {
            for (float y = minPoint.y; y <= maxPoint.y; y += occupancyCellResolution)
            {
                for (float z = minPoint.z; z <= maxPoint.z; z += occupancyCellResolution)
                {
                    Vector3 queryPoint = new Vector3(x, y, z);
                    occupiedCells.Add(QuantizeToOccupancyGrid(queryPoint));
                }
            }
        }

        // Mark points along path as occupied
        // TODO: Need to mark points along radius as occupied? 
        float pathOccupancyStepDistance = occupancyCellResolution / 2;
        for (float distance = 0; distance < path.length; distance += pathOccupancyStepDistance)
        {
            Vector3 occupancyQueryPoint = path.GetPointAtDistance(distance, EndOfPathInstruction.Stop);
            Vector3 quantizedQueryPoint = QuantizeToOccupancyGrid(occupancyQueryPoint);
            Debug.Log($"Path now occupying cell {quantizedQueryPoint}");
            occupiedCells.Add(quantizedQueryPoint);
        }
    }

    private Vector3 QuantizeToOccupancyGrid(Vector3 queryPosition)
    {
        return new Vector3(
            Mathf.Round(queryPosition.x / occupancyCellResolution) * occupancyCellResolution,
            Mathf.Round(queryPosition.y / occupancyCellResolution) * occupancyCellResolution,
            Mathf.Round(queryPosition.z / occupancyCellResolution) * occupancyCellResolution
        );
    }
}
