using DG.Tweening;
using FateGames.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DataEditController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText = null;
    [SerializeField] private TextMeshProUGUI levelText = null;
    [SerializeField] private SaveDataVariable saveData = null;
    [SerializeField] private MoneyUI moneyUI = null;
    [SerializeField] private GameObject UIParent = null;

    public void SetMoney()
    {
        moneyUI.SetMoneyFromDevMode(moneyText.text);
    }

    public void ToggleUI()
    {
        UIParent.SetActive(!UIParent.activeSelf);
    }

    public void SetLevel()
    {
        try
        {
            int result = int.Parse(levelText.text.Substring(0, levelText.text.Length - 1));
            saveData.Value.Level = result;
            GameManager.Instance.LoadCurrentLevel();
            print("level updated");
        }
        catch (FormatException)
        {
            print("Unable to parse");
        }
    }

}
