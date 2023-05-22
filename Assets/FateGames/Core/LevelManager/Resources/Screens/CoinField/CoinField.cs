using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using TMPro;

public class CoinField : FateMonoBehaviour
{
    [SerializeField] private SaveDataVariable saveData;
    [SerializeField] private TextMeshProUGUI coinText;

    private void Start()
    {
        UpdateField();
    }
    public void UpdateField()
    {
        coinText.text = saveData.Value.Coin.ToString();
    }
}

public partial class SaveData
{
    public int Coin = 0;
}