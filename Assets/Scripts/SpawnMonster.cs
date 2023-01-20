using System.Runtime.CompilerServices;
using UnityEngine;

public class SpawnMonster : MonoBehaviour
{
    
    /// <summary>
    /// Monster object with collider and nav agent to add to scene at <see cref="spawnTargetGO"/> transform
    /// </summary>
    [SerializeField] public GameObject monsterPrefab;


    private bool isSpawned = false;
    private string parentName;
    // Start is called before the first frame update
    void Start()
    {
        

    }


    void Update()
    {
        if (!isSpawned)
        { 
            // Instantiate monsterPrefab at spawnTargetGO transform
            Instantiate(monsterPrefab, gameObject.transform.position, Quaternion.identity);
            Debug.Log(monsterPrefab.name + " spawned at " + gameObject.name + " : " + gameObject.transform.position);


            // Self destruct
            parentName = gameObject.name;
            Destroy(gameObject);
            Debug.Log(monsterPrefab.name + " created successfully. Destroyed " + parentName + " parent object.");
        }

    }
}

