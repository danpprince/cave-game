using UnityEngine;
using UnityEngine.AI;

public class FollowPlayer : MonoBehaviour
{
    private NavMeshAgent agent;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();

    }

    // Update is called once per frame
    void Update()
    {
        // Find the player by tag
        player = GameObject.FindGameObjectWithTag("Player");

        // Find a point on the NavMesh near the target
        NavMeshHit hit;
        NavMesh.SamplePosition(player.transform.position, out hit, 2.0f, NavMesh.AllAreas);
        //Debug.Log("Current target " + player.name + " is at " + hit.position);
        
        // Set the agent's destination to the target
        agent.destination = hit.position;
        
        // Check if the agent has reached its destination
        if (agent.remainingDistance < 0.1f)
        {
            //Debug.Log(transform.name + " has reached " + player.name);
            
        }
        else
        {
            // Debug.Log(transform.name + " is moving towards " + player.name);
        }
    }

}
