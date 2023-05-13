using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartSceneOnPlayerCollide : MonoBehaviour
{
    public string playerTag;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag(playerTag))
        {
            print("Restarting level, collided with player");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
