using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FateGames.Core;
using DG.Tweening;
using UnityEngine.Events;

public class XPanel : MonoBehaviour
{
    [SerializeField] private SaveDataVariable saveData;
    [SerializeField] private float speed = 1;
    [SerializeField] private RectTransform cursor;
    [SerializeField] private TextMeshProUGUI coinText, claimText;
    [SerializeField] private int baseCoin = 15;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private RectTransform from, to;
    [SerializeField] private UnityEvent onCoinAdded;
    [SerializeField] private UnityEvent onRewardGiven;
    [SerializeField] private SoundEntity coinSound;
    private int coin = 0;
    private float multiplier = 1;
    private bool claimed = false;

    private void Update()
    {
        if (claimed) return;
        float value = Mathf.Sin(Time.time * speed);
        cursor.anchoredPosition = new Vector2(value * 291, cursor.anchoredPosition.y);

        if (value > 0.6f) multiplier = 2;
        else if (value > 0.2f) multiplier = 3;
        else if (value > -0.2f) multiplier = 5;
        else if (value > -0.6f) multiplier = 3;
        else multiplier = 2;
        coin = Mathf.CeilToInt(baseCoin * multiplier);
        coinText.text = "+" + coin;
        claimText.text = "Claim " + multiplier + "X";
    }

    public void Claim()
    {
        GameManager.Instance.PlayHaptic();
        claimed = true;
        SDKManager.Instance.ShowRewardedAd(() => { }, GiveReward);
    }

    public void GiveReward()
    {
        MoneyUI.Instance.BurstFlyingMoney(coin, 20, Screen.width / 3, from.position);
        onRewardGiven.Invoke();
        DOVirtual.DelayedCall(3f, () => GameManager.Instance.LoadCurrentLevel());
    }

    public void NoThanks()
    {
        GameManager.Instance.PlayHaptic();
        int coin = Mathf.CeilToInt(baseCoin);
        MoneyUI.Instance.BurstFlyingMoney(coin, 10, Screen.width / 3, from.position);
        DOVirtual.DelayedCall(2.5f, () => GameManager.Instance.LoadCurrentLevel());
    }
}
