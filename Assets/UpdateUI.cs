using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpdateUI : MonoBehaviour
{
    public TextMeshProUGUI coinsCollectedText;

    void Update()
    {
        coinsCollectedText.text = 
            $"{GameManager.GetNumCoinsCollected()} / {GameManager.GetNumTotalCoins()} coins";
    }
}
