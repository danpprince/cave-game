using UnityEngine.AI;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    public NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
                
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the agent has reached its destination
        if (agent.remainingDistance < 0.1f )
        {
            // Move the agent to a new random position
            agent.destination = RandomNavSphere(transform.position, 100, -1);
        }
        Debug.DrawLine(agent.transform.position, agent.destination, Color.magenta, 0.01f);
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
