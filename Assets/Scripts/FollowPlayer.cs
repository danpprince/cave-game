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

        // If the player is found, follow the player
        if (player != null)
        {
            agent.SetDestination(player.transform.position);
        }

    }

}
