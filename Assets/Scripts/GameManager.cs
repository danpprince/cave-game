using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string coinTag;
    private static string coinTagStatic;
    private static int numCoinsCollected;
    private static int numTotalCoins;

    void Start()
    {
        // Do after GenerateLevelOnStart?
        // Race condition here, eww
        coinTagStatic = coinTag;
        Application.targetFrameRate = 60;
    }

    public static void CountCoinsInLevel()
    {
        numCoinsCollected = 0;
        numTotalCoins = GameObject.FindGameObjectsWithTag(coinTagStatic).Length;
        Debug.Log($"Found {numTotalCoins} coins in level");
    }

    public static void RestartState()
    {
        numCoinsCollected = 0;
        numTotalCoins = 0;
    }

    public static void IncrementNumCoinsCollected()
    {
        numCoinsCollected++;
        Debug.Log($"{numCoinsCollected} of {numTotalCoins} coins collected");
    }

    public static int GetNumCoinsCollected() { return numCoinsCollected; }
    public static int GetNumTotalCoins() { return numTotalCoins; }
}
