using DG.Tweening;
using FateGames.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private RectTransform imageTransform;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private RectTransform canvas;
    [SerializeField] private SaveDataVariable saveData;
    [SerializeField] private GameObject flyingMoneyPrefab = null;
    [SerializeField] private GameEvent onMoneyChanged = null;
    [SerializeField] private SoundEntity burstSound = null;
    private bool animating = false;

    public Vector2 MoneyTarget { get => imageTransform.position; }

    public int Money { get => saveData.Value.Money; }

    private FateObjectPool<FlyingMoney> flyingMoneyPool;

    public static MoneyUI Instance = null;
    private void Awake()
    {
        Instance = this;
        flyingMoneyPool = new FateObjectPool<FlyingMoney>(flyingMoneyPrefab, true, 50, 100);
    }

    private void Start()
    {
        UpdateUI();
    }

    /*private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            BurstFlyingMoney(1000, 15, Screen.width / 3f, new Vector2(Screen.width / 2, Screen.height / 2));
    }*/


    public void AddMoney(int amount)
    {
        saveData.Value.Money += amount;
        UpdateUI();
    }

    public void SpendMoney(int amount)
    {
        saveData.Value.Money -= amount;
        UpdateUI();
    }

    public void DirectFlyingMoney(int amount, Vector2 spawnPosition)
    {
        FlyingMoney money = MakeMoney();
        money.DirectGoToUI(amount, spawnPosition);
    }

    public void BurstFlyingMoney(int amount, int count, float radius, Vector2 spawnPosition)
    {
        GameManager.Instance.PlaySound(burstSound);
        int valueOfSingleMoneyImage = amount / count;
        int remainder = amount - (valueOfSingleMoneyImage * count);

        for (int i = 0; i < count; i++)
        {
            FlyingMoney money = MakeMoney();

            int gain = valueOfSingleMoneyImage;
            if (i == count - 1) gain += remainder;

            float randomAngle = UnityEngine.Random.Range(0, 360);
            float distance = UnityEngine.Random.Range(0, 1f) * radius;

            Vector2 midPosition = spawnPosition + new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * distance;

            money.GoUIWithBurstMove(gain, spawnPosition, midPosition);
        }
    }

    public void SetMoneyFromDevMode(string money)
    {
        try
        {
            int result = int.Parse(money.Substring(0, money.Length - 1));
            saveData.Value.Money = result;
            UpdateUI();
            print("money updated");
        }
        catch (FormatException)
        {
            print("Unable to parse");
        }
    }

    private FlyingMoney MakeMoney()
    {
        FlyingMoney money = flyingMoneyPool.Get();
        if (money.transform.parent != canvas) money.transform.SetParent(canvas);
        money.transform.localScale = Vector3.one;
        return money;
    }

    private void UpdateUI()
    {
        moneyText.text = numberFormat(saveData.Value.Money).Replace(",", ".");
        onMoneyChanged.Raise();
        BounceMoney();
    }

    private void BounceMoney()
    {
        if (animating) return;
        animating = true;
        imageTransform.DOScale(1.2f, 0.05f).SetLoops(2, LoopType.Yoyo).OnComplete(() => { animating = false; });
    }

    public enum suffixes
    {
        p, // p is a placeholder if the value is under 1 thousand
        K, // Thousand
        M, // Million
        B, // Billion
        T, // Trillion
        Q, // Quadrillion
    }

    public static string numberFormat(long money)
    {
        int decimals = 2; //How many decimals to round to
        string r = money.ToString(); //Get a default return value

        foreach (suffixes suffix in Enum.GetValues(typeof(suffixes))) //For each value in the suffixes enum
        {
            var currentVal = 1 * Math.Pow(10, (int)suffix * 3); //Assign the amount of digits to the base 10
            var suff = Enum.GetName(typeof(suffixes), (int)suffix); //Get the suffix value
            if ((int)suffix == 0) //If the suffix is the p placeholder
                suff = String.Empty; //set it to an empty string

            if (money >= currentVal)
                r = Math.Round((money / currentVal), decimals, MidpointRounding.ToEven).ToString() + suff; //Set the return value to a rounded value with suffix
            else
                return r; //If the value wont go anymore then return
        }
        return r; // Default Return
    }
}

public partial class SaveData
{
    public int Money = 0;
}


