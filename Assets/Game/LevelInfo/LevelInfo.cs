using FateGames.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText = null;
    [SerializeField] private SaveDataVariable saveData;

    private void Awake()
    {
        levelText.text = "Level " + saveData.Value.Level.ToString();
    }
}
