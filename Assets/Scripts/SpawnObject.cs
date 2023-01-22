using System.Runtime.CompilerServices;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{

    /// <summary>
    /// Object to spawn at parent location
    /// </summary>
    [SerializeField] public GameObject objectPrefab;

    [SerializeField] private bool disableDestroy = true;

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
            Instantiate(objectPrefab, gameObject.transform.position, Quaternion.identity);
            //Debug.Log(objectPrefab.name + " spawned at " + gameObject.name + " : " + gameObject.transform.position);
            isSpawned = true;

            if (!disableDestroy)
            {
                // Self destruct
                parentName = gameObject.name;
                Destroy(gameObject);
                Debug.Log("Destroyed " + parentName + " parent object.");
            }
        }

    }
}

