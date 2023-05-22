using FateGames.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticItem : MonoBehaviour
{
    [SerializeField] private IntReference itemNo;
    [SerializeField] private GameObject[] cosmetics;

    private int lastCosmetic = 0;

    private void Awake()
    {
        UpdateCosmetic();
    }

    public void UpdateCosmetic()
    {
        cosmetics[lastCosmetic].SetActive(false);
        cosmetics[itemNo.Value].SetActive(true);
        lastCosmetic = itemNo.Value;
    }
}
