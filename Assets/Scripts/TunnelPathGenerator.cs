using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PathCreation;

/// <summary>
/// Generates tunnels recursively consisting of paths and room triggers (not meshes)
/// </summary>
public class TunnelPathGenerator : MonoBehaviour
{
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

    /// <summary>
    /// Maximum number depth of recursive calls for creating new rooms
    /// </summary>
    public int maxRecursionDepth;

    /// <summary>
    /// Probability that each tunnel endpoint will be used for a recursive call (between 0.0 and 1.0)
    /// </summary>
    public float recursionProbability;

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

    /// <summary>
    /// Reference to new object that will parent all newly generated paths and rooms
    /// </summary>
    private GameObject generatedParent;

    /// <summary>
    /// Create tunnel paths with Bezier curves and colliders
    /// </summary>
    public void GeneratePaths()
    {
        generatedParent = new GameObject("Generated");
        generatedParent.transform.parent = gameObject.transform;
        Vector3 startingPosition = startingTunnelEndpoint.position;
        Vector3 baseDirection = (startingPosition - startingRoomTransform.position).normalized;
        GeneratePathsRecursive(startingPosition, baseDirection, 0);
    }

    /// <summary>
    /// Generate one new collider "room" from a source and recursively call for the next rooms
    /// </summary>
    /// <param name="sourcePosition">Starting position where the new path should originate</param>
    /// <param name="baseDirection">General direction the new room should be created in relative to sourceTransform</param>
    /// <param name="depth">, should be 0 initially</param>
    private void GeneratePathsRecursive(Vector3 sourcePosition, Vector3 baseDirection, int depth)
    {
        if (depth >= maxRecursionDepth)
        {
            return;
        }

        GameObject destinationRoom = MakeDestinationRoom(sourcePosition, baseDirection);
        (Transform pathDestination, List<Transform> childTransformsToRecurseInto) = 
            GetRoomEndpoints(destinationRoom, sourcePosition);
        MakePathToDestination(sourcePosition, pathDestination.position);

        foreach (Transform childTransform in childTransformsToRecurseInto)
        {
            // Skip recursion randomly or if the current child is the same as the completed tunnel destination
            if (Random.value > recursionProbability || childTransform == pathDestination)
            {
                continue;
            }
            Vector3 childPosition = childTransform.position;
            Vector3 childBaseDirection =(childPosition - destinationRoom.transform.position).normalized;
            GeneratePathsRecursive(childPosition, childBaseDirection, depth + 1);
        }
    }

    /// <summary>
    /// Create a new GameObject for a destination room
    /// </summary>
    /// <param name="sourcePosition"></param>
    /// <param name="baseDirection"></param>
    /// <returns></returns>
    private GameObject MakeDestinationRoom(Vector3 sourcePosition, Vector3 baseDirection)
    {
        float tunnelLength = Random.Range(tunnelLengthMin, tunnelLengthMax);
        Vector3 roomPosition = sourcePosition + baseDirection * tunnelLength;
        float downwardAngle = Random.Range(0, tunnelDownwardAngleMaxDegrees);
        float yChange = tunnelLength * Mathf.Tan(downwardAngle * Mathf.Deg2Rad);
        roomPosition.y += Random.Range(-yChange, 0f);

        Quaternion roomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        GameObject room = Instantiate(roomPrefab, roomPosition, roomRotation, generatedParent.transform);

        return room;
    }

    /// <summary>
    /// 
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
                pathDistance = Vector3.Distance(sourcePosition, sourcePosition);
                print($"Closest destination was null, now {closestTransform} with distance {pathDistance}");
                continue;
            }

            float newTunnelDistance = Vector3.Distance(sourcePosition, childObject.transform.position);
            if (newTunnelDistance < pathDistance)
            {
                closestTransform = childObject.transform;
                pathDistance = Vector3.Distance(sourcePosition, sourcePosition);
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
    private void MakePathToDestination(Vector3 sourcePosition, Vector3 destinationPosition)
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

        PathCreator tunnelPathCreator = pathObject.AddComponent<PathCreator>();
        tunnelPathCreator.bezierPath = new BezierPath(pathPoints);

        // Toggle control mode so automatic tangent points are set correctly
        tunnelPathCreator.bezierPath.ControlPointMode = BezierPath.ControlMode.Aligned;
        tunnelPathCreator.bezierPath.ControlPointMode = BezierPath.ControlMode.Automatic;
    }
}
