using UnityEngine;
using UnityEngine.AI;

public class MinotaurBehavior : MonoBehaviour
{
    private NavMeshAgent agent;
    private GameObject player;

    [SerializeField] private bool disableDestroy = false;

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

        // If the player is found, follow the player
        if (player != null)
        {
            agent.SetDestination(player.transform.position);            
        }
        else
        {
            // Move the agent to a new random position
            agent.destination = RandomNavSphere(transform.position, 100, NavMesh.AllAreas);
        }

        for (int i = 0; i < agent.path.corners.Length - 1; i++)
        {
            Debug.DrawLine(agent.path.corners[i], agent.path.corners[i + 1], Color.magenta);
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

    // Destroys Player on collision
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !disableDestroy)
        {        
            Debug.Log("Player has been destroyed by " + gameObject.name);
            Destroy(other.gameObject);
        }
        else
            Debug.Log("Player has been caught by " + gameObject.name + " but not destroyed");
    }
}
