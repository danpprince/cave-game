using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string coinTag;
    private static int numCoinsCollected;
    private static int numTotalCoins;

    void Start()
    {
        Application.targetFrameRate = 60;
        // TODO: How to make this work in editor procedural generation and scene start generation?
        numTotalCoins = GameObject.FindGameObjectsWithTag("Coin").Length;
    }

    public static void RestartState()
    {
        numCoinsCollected = 0;
        numTotalCoins = 0;
    }

    public static void IncrementNumCoinsInLevel()
    {
        numTotalCoins++;
        Debug.Log($"{numTotalCoins} coins in level so far");
    }

    public static void IncrementNumCoinsCollected()
    {
        numCoinsCollected++;
        Debug.Log($"{numCoinsCollected} of {numTotalCoins} coins collected");
    }

    public static int GetNumCoinsCollected() { return numCoinsCollected; }
    public static int GetNumTotalCoins() { return numTotalCoins; }
}
