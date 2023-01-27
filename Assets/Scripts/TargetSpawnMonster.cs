using UnityEngine;

public class TargetSpawnMonster : MonoBehaviour
{
    /// <summary>
    /// Prefab object to provide transform to locate the new <see cref="monsterPrefab"/>
    /// </summary>
    [SerializeField] public GameObject spawnTargetGO;

    /// <summary>
    /// Monster object with collider and nav agent to add to scene at <see cref="spawnTargetGO"/> transform
    /// </summary>
    [SerializeField] public GameObject monsterPrefab;


    private GameObject[] targetList;

    // Start is called before the first frame update
    void Start()
    {
        

    }


    void Update()
    {
        // Find name of spawnTargetGO by tag
        string targetTag = spawnTargetGO.tag;
        
        if (targetList == null)
        {
            targetList = GameObject.FindGameObjectsWithTag(targetTag);
            
            // Count number of targets in targetList and calculate random index
            int targetCount = targetList.Length;
            //Debug.Log("Found " + targetCount + " targets with tag: " + spawnTargetGO.tag);

            int spawnIndex = Random.Range(0, targetCount - 1);
            //Debug.Log("Spawn index: " + spawnIndex);

            // Instantiate monsterPrefab at spawnTargetGO transform
            Instantiate(monsterPrefab, targetList[spawnIndex].transform.position, Quaternion.identity);
            Debug.Log(monsterPrefab.name + " spawned at " + targetList[spawnIndex].name + " : " + targetList[spawnIndex].transform.position);
        }

    }
}

