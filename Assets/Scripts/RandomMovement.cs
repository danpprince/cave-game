using UnityEngine;
using UnityEngine.AI;

public class RandomMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();

    }

    // Update is called once per frame
    void Update()
    {
        // Check if the agent has reached its destination
        if (agent.remainingDistance < 0.1f)
        {
            // Move the agent to a new random position
            agent.destination = RandomNavSphere(transform.position, 100, NavMesh.AllAreas);
        }
        
        // Draw the path the agent is following
        for (int i = 0; i < agent.path.corners.Length - 1; i++)
        {
            Debug.DrawLine(agent.path.corners[i], agent.path.corners[i + 1], Color.yellow);
        }
    }

    // Returns a random position within sphere on NavMesh
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }





}
