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
        Vector3 position = new Vector3(0, 20, 0);
        Quaternion rotation = Quaternion.Euler(0, -150, 0);
        GameObject room = Instantiate(roomPrefab, position, rotation, transform);

        Transform pathDestination = transform;
        for (int childIndex = 0; childIndex < room.transform.childCount; childIndex++)
        {
            GameObject childObject = room.transform.GetChild(childIndex).gameObject;
            if (childObject.tag == tunnelEndpointTag)
            {
                print($"Found tunnel endpoint {childObject.name}, setting as destination");
                pathDestination = childObject.transform;
                break;
            }
        }

        GameObject pathObject = new GameObject("Path");
        pathObject.transform.parent = gameObject.transform;
        Vector3 midpoint = new Vector3(-15, 23, 15);
        List<Vector3> pathPoints = new List<Vector3> {
            startingTunnelEndpoint.position, 
            midpoint,
            pathDestination.position
        };
        PathCreator tunnelPathCreator = pathObject.AddComponent<PathCreator>();
        tunnelPathCreator.bezierPath = new BezierPath(pathPoints);

        // Toggle control mode so automatic tangent points are set correctly
        tunnelPathCreator.bezierPath.ControlPointMode = BezierPath.ControlMode.Aligned;
        tunnelPathCreator.bezierPath.ControlPointMode = BezierPath.ControlMode.Automatic;
    }
}
