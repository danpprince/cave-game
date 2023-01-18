using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMonster : MonoBehaviour
{

    [SerializeField] public GameObject spawnTargetGO;

    /// <summary>
    /// Monster object with collider and nav agents to add at to scene at spawnTarget transform
    /// </summary>
    [SerializeField] public GameObject monsterPrefab;
        
    // Start is called before the first frame update
    void Start()
    {
        // Find name of spawnTargetGO
        
        // Find all other objects with same name as spawnTargetGO

        // Randomly select one object of such name

        // Instantiate monsterPrefab at transform of randomly selected spawn target


    }
}
