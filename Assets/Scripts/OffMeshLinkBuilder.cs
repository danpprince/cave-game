using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class OffMeshLinkBuilder : MonoBehaviour
{
    private NavMeshAgent agent;
    
    //private GameObject player;

    private List<Vector3> edgeVertices;

    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
    }
    // Update is called once per frame
    void Update()
    {
        NavMeshHit nextEdgeHit = new NavMeshHit();

        // If the list of edgeVertices is null, create a new list and add the first vertex
        if (edgeVertices == null || edgeVertices.Count == 0)
        {
            // Find NavMeshEdge nearest to agent and add to list of edge vertices
            NavMesh.FindClosestEdge(agent.transform.position, out nextEdgeHit, NavMesh.AllAreas);
            edgeVertices.Add(nextEdgeHit.position);
        }

        // If the list of edgeVertices has more than 1 vertex
        if (edgeVertices.Count > 1)
        { 
            // Find NavMeshEdge at least 2 units away from agent
            NavMesh.FindClosestEdge(agent.transform.position + agent.transform.forward * 2, out nextEdgeHit, NavMesh.AllAreas);

            // If nextEdgeHit is not already in list of edge vertices, add it
            if (!edgeVertices.Contains(nextEdgeHit.position))
            {
                edgeVertices.Add(nextEdgeHit.position);
            }

            // Draw a line between the current edge vertex and the last edge vertex
            Debug.DrawLine(nextEdgeHit.position, edgeVertices[edgeVertices.LastIndexOf(nextEdgeHit.position) - 1] , Color.magenta);
        }
        
        // Move the agent to the next edge vertex
        agent.destination = nextEdgeHit.position;

    }

}
