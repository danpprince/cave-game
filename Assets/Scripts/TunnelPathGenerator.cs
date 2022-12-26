using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PathCreation;

public class TunnelPathGenerator : MonoBehaviour
{
    public Transform startingRoomTransform;
    public Transform startingTunnelEndpoint;
    public GameObject roomPrefab;

    public string tunnelEndpointTag;
    public int maxRecursionDepth;
    public float recursionProbability; // [0, 1]
    public float tunnelLengthMin, tunnelLengthMax;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GeneratePaths()
    {
        Vector3 baseDirection = (startingTunnelEndpoint.position - startingRoomTransform.position).normalized;
        GeneratePathsRecursive(startingTunnelEndpoint, baseDirection, 0);

    }

    private void GeneratePathsRecursive(Transform sourceTunnelTransform, Vector3 baseDirection, int depth)
    {
        if (depth >= maxRecursionDepth)
        {
            return;
        }

        Vector3 sourceTunnelPosition = sourceTunnelTransform.position;

        // Deeper rooms always go down in Y, but can be positive or negative in X and Z
        //float xSign = Random.value < 0.5f ? 1 : -1;
        //float zSign = Random.value < 0.5f ? 1 : -1;
        //Vector3 destinationRoomPosition = new Vector3(
        //    sourceTunnelPosition.x + xSign * Random.Range(10, 20),
        //    sourceTunnelPosition.y + Random.Range(-5, 0),
        //    sourceTunnelPosition.z + zSign * Random.Range(10, 20)
        //);
        Vector3 destinationRoomPosition = 
            sourceTunnelTransform.position
            + baseDirection * Random.Range(tunnelLengthMin, tunnelLengthMax);
        destinationRoomPosition.y += Random.Range(-5f, 0f);
        Quaternion destinationRoomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        GameObject room = Instantiate(roomPrefab, destinationRoomPosition, destinationRoomRotation, transform);
        room.transform.parent = sourceTunnelTransform;

        // Set the path destination to the closest TunnelEndpoint child in the new room
        Transform pathDestination = null;
        float pathDistance = float.PositiveInfinity;
        List<Transform> childTransformsToRecurseInto = new List<Transform>();
        for (int childIndex = 0; childIndex < room.transform.childCount; childIndex++)
        {
            GameObject childObject = room.transform.GetChild(childIndex).gameObject;
            if (childObject.tag != tunnelEndpointTag)
            {
                continue;
            }

            childTransformsToRecurseInto.Add(childObject.transform);

            if (pathDestination is null)
            {
                pathDestination = childObject.transform;
                pathDistance = Vector3.Distance(sourceTunnelPosition, pathDestination.position);
                print($"Path destination was null, now {pathDestination} with distance {pathDistance}");
                continue;
            }

            float newTunnelDistance = Vector3.Distance(sourceTunnelPosition, childObject.transform.position);
            if (newTunnelDistance < pathDistance)
            {
                pathDestination = childObject.transform;
                pathDistance = Vector3.Distance(sourceTunnelPosition, pathDestination.position);
                print($"Shorter path found, now destination {pathDestination} with distance {pathDistance}");
                continue;
            }
        }

        // Create the bezier path to the selected destination
        GameObject pathObject = new GameObject("Path");
        pathObject.transform.parent = sourceTunnelTransform.transform;
        List<Vector3> pathPoints = new List<Vector3> {
            sourceTunnelPosition, pathDestination.position
        };
        PathCreator tunnelPathCreator = pathObject.AddComponent<PathCreator>();
        tunnelPathCreator.bezierPath = new BezierPath(pathPoints);

        // Toggle control mode so automatic tangent points are set correctly
        tunnelPathCreator.bezierPath.ControlPointMode = BezierPath.ControlMode.Aligned;
        tunnelPathCreator.bezierPath.ControlPointMode = BezierPath.ControlMode.Automatic;

        foreach (Transform childTransform in childTransformsToRecurseInto)
        {
            if (Random.value > recursionProbability)
            {
                continue;
            }
            Vector3 childBaseDirection = (childTransform.position - room.transform.position).normalized;
            GeneratePathsRecursive(childTransform, childBaseDirection, depth + 1);
        }
    }
}
