using UnityEngine;

public class SceneRestarter : MonoBehaviour

{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Check if there is a player in the scene
        if (GameObject.Find("Player") == null)
        {
            Debug.Log("The Player is dead! Resetting Scene...");

            // If there is no player, load the game over scene
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
            
        }

    }
}
