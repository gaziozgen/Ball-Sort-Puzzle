using FateGames.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CosmeticOption : MonoBehaviour
{
    [SerializeField] private int categoryIndex = -1;
    [SerializeField] private int cosmeticIndex = -1;
    [SerializeField] private GameObject selectedOutline;
    [SerializeField] private GameObject lockedBarrier;
    [SerializeField] private SaveDataVariable saveData;
    [SerializeField] private IntVariable currentSelectedCosmetic;

    public int CosmeticIndex { get => cosmeticIndex; }

    public bool IsUnlocked { get => saveData.Value.CosmeticAchivedTable[categoryIndex, cosmeticIndex] == true; }

    private void Awake()
    {
        if (!IsUnlocked)
        {
            lockedBarrier.SetActive(true);
        }
        UpdateSelected();
    }

    public void Tap()
    {
        if (IsUnlocked) CosmeticPanel.Instance.SelectCosmetic(this);
    }

    public void UpdateSelected()
    {
        selectedOutline.SetActive(currentSelectedCosmetic.Value == cosmeticIndex);
    }

    public void Buy()
    {
        saveData.Value.CosmeticAchivedTable[categoryIndex, cosmeticIndex] = true;
        Unlock();
    }

    private void Unlock()
    {
        lockedBarrier.SetActive(false);
    }


}
