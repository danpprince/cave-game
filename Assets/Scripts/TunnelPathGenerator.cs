using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PathCreation;

public class TunnelPathGenerator : MonoBehaviour
{
    public Transform startingTunnelEndpoint;
    public GameObject roomPrefab;

    public string tunnelEndpointTag;

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
        Transform sourceTunnelPoint = startingTunnelEndpoint;

        Vector3 destinationRoomPosition = new Vector3(0, 20, 0);
        Quaternion destinationRoomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        GameObject room = Instantiate(roomPrefab, destinationRoomPosition, destinationRoomRotation, transform);

        // Set the path destination to the closest TunnelEndpoint child in the new room
        Transform pathDestination = null;
        float pathDistance = float.PositiveInfinity;
        for (int childIndex = 0; childIndex < room.transform.childCount; childIndex++)
        {
            GameObject childObject = room.transform.GetChild(childIndex).gameObject;
            if (childObject.tag != tunnelEndpointTag)
            {
                continue;
            }
            if (pathDestination is null)
            {
                pathDestination = childObject.transform;
                pathDistance = Vector3.Distance(sourceTunnelPoint.position, pathDestination.position);
                print($"Path destination was null, now {pathDestination} with distance {pathDistance}");
                continue;
            }

            float newTunnelDistance = Vector3.Distance(sourceTunnelPoint.position, childObject.transform.position);
            if (newTunnelDistance < pathDistance)
            {
                pathDestination = childObject.transform;
                pathDistance = Vector3.Distance(sourceTunnelPoint.position, pathDestination.position);
                print($"Shorter path found, now destination {pathDestination} with distance {pathDistance}");
                continue;
            }
        }

        GameObject pathObject = new GameObject("Path");
        pathObject.transform.parent = gameObject.transform;
        List<Vector3> pathPoints = new List<Vector3> {
            sourceTunnelPoint.position, 
            pathDestination.position
        };
        PathCreator tunnelPathCreator = pathObject.AddComponent<PathCreator>();
        tunnelPathCreator.bezierPath = new BezierPath(pathPoints);

        // Toggle control mode so automatic tangent points are set correctly
        tunnelPathCreator.bezierPath.ControlPointMode = BezierPath.ControlMode.Aligned;
        tunnelPathCreator.bezierPath.ControlPointMode = BezierPath.ControlMode.Automatic;
    }
}
