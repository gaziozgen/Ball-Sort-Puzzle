using FateGames.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CosmeticPanel : MonoBehaviour
{
    [SerializeField] private int randomCosmeticCost = 500;
    [SerializeField] private int watchAdRevanue = 500;
    [SerializeField] private float watchAdCooldown = 300;
    [SerializeField] private List<CosmeticOption> ballCosmetics = null;
    [SerializeField] private List<CosmeticOption> tubeCosmetics = null;
    [SerializeField] private List<CosmeticOption> plugCosmetics = null;
    [SerializeField] private List<CosmeticOption> backgroundCosmetics = null;
    [SerializeField] private Canvas panelCanvas = null;
    [SerializeField] private GameObject[] categoryButtonsHighlights;
    [SerializeField] private GameObject[] cetegories;
    [SerializeField] private IntVariable[] selectedCosmeticVariables;
    [SerializeField] private GameEvent[] cosmeticChangedEvents;
    [SerializeField] private SaveDataVariable saveData;
    [SerializeField] private SoundEntity sound;

    [SerializeField] private Button buyRandomButton = null;
    [SerializeField] private TextMeshProUGUI randomCosmeticCostText = null;

    [SerializeField] private Button adButton = null;
    [SerializeField] private GameObject adButtonCostArea = null;
    [SerializeField] private GameObject adButtonTimerArea = null;
    [SerializeField] private TextMeshProUGUI watchAdRevanueText = null;
    [SerializeField] private TextMeshProUGUI watchAdTimerText = null;

    private int lastCategory = 0;
    private List<List<CosmeticOption>> allCosmetics = new();
    private float countdownEnd = -1;

    public static CosmeticPanel Instance = null;
    private void Awake()
    {
        Instance = this;
        allCosmetics.Add(ballCosmetics);
        allCosmetics.Add(tubeCosmetics);
        allCosmetics.Add(plugCosmetics);
        allCosmetics.Add(backgroundCosmetics);
        for (int i = 0; i < cetegories.Length; i++)
        {
            selectedCosmeticVariables[i].Value = saveData.Value.LastSelectedCosmetics[i];
            cosmeticChangedEvents[i].Raise();
        }
        watchAdRevanueText.text = "+" + watchAdRevanue.ToString();
        randomCosmeticCostText.text = randomCosmeticCost.ToString();
    }

    public void Update()
    {
        if (Time.time < countdownEnd) watchAdTimerText.text = FormatTime(countdownEnd - Time.time);
        else if (adButton.interactable == false) UpdateWatchAdButton();
    }

    public void Open()
    {
        GameManager.Instance.PlayHaptic();
        GameManager.Instance.PlaySound(sound);
        panelCanvas.enabled = true;
        UpdateButtons();
    }

    public void Close()
    {
        GameManager.Instance.PlayHaptic();
        GameManager.Instance.PlaySound(sound);
        panelCanvas.enabled = false;
    }

    public void SelectCategory(int category)
    {
        GameManager.Instance.PlayHaptic();
        GameManager.Instance.PlaySound(sound);
        categoryButtonsHighlights[lastCategory].SetActive(false);
        cetegories[lastCategory].SetActive(false);

        lastCategory = category;
        categoryButtonsHighlights[category].SetActive(true);
        cetegories[category].SetActive(true);
        UpdateButtons();
    }

    public void BuyRandomCosmetic()
    {
        GameManager.Instance.PlayHaptic();
        GameManager.Instance.PlaySound(sound);
        List<CosmeticOption> lockedCosmetics = new();
        for (int i = 0; i < allCosmetics[lastCategory].Count; i++)
            if (!allCosmetics[lastCategory][i].IsUnlocked) lockedCosmetics.Add(allCosmetics[lastCategory][i]);

        if (MoneyUI.Instance.Money >= randomCosmeticCost)
        {
            MoneyUI.Instance.SpendMoney(randomCosmeticCost);
            lockedCosmetics[Random.Range(0, lockedCosmetics.Count)].Buy();
        }
        UpdateButtons();
    }

    public void WatchAdForMoney()
    {
        GameManager.Instance.PlayHaptic();
        GameManager.Instance.PlaySound(sound);

        SDKManager.Instance.ShowRewardedAd(() => { }, () =>
        {
            MoneyUI.Instance.BurstFlyingMoney(watchAdRevanue, 20, Screen.width / 3, new Vector2(Screen.width / 2, Screen.height / 2));
            countdownEnd = Time.time + watchAdCooldown;
            UpdateButtons();
        });
        
    }

    private void UpdateButtons()
    {
        UpdateRandomBuyButton();
        UpdateWatchAdButton();
    }

    public void UpdateRandomBuyButton()
    {
        List<CosmeticOption> lockedCosmetics = new();
        for (int i = 0; i < allCosmetics[lastCategory].Count; i++)
            if (!allCosmetics[lastCategory][i].IsUnlocked) lockedCosmetics.Add(allCosmetics[lastCategory][i]);

        if (lockedCosmetics.Count > 0)
        {
            buyRandomButton.gameObject.SetActive(true);
            if (MoneyUI.Instance.Money >= randomCosmeticCost)
                buyRandomButton.interactable = true;
            else
                buyRandomButton.interactable = false;
        }
        else
            buyRandomButton.gameObject.SetActive(false);
    }

    private void UpdateWatchAdButton()
    {
        if (Time.time > countdownEnd)
        {
            adButton.interactable = true;
            if (!adButtonCostArea.activeSelf) adButtonCostArea.SetActive(true);
            if (adButtonTimerArea.activeSelf) adButtonTimerArea.SetActive(false);
        }
        else
        {
            adButton.interactable = false;
            if (adButtonCostArea.activeSelf) adButtonCostArea.SetActive(false);
            if (!adButtonTimerArea.activeSelf) adButtonTimerArea.SetActive(true);
            
        }
    }

    public void SelectCosmetic(CosmeticOption cosmetic)
    {
        GameManager.Instance.PlayHaptic();
        selectedCosmeticVariables[lastCategory].Value = cosmetic.CosmeticIndex;
        saveData.Value.LastSelectedCosmetics[lastCategory] = cosmetic.CosmeticIndex;
        cosmeticChangedEvents[lastCategory].Raise();
    }

    private string FormatTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        return minutes.ToString() + ":" + seconds.ToString();
    }
}


public partial class SaveData
{
    public int[] LastSelectedCosmetics = { 0, 0, 0, 0 };
    public bool[,] CosmeticAchivedTable = {
        { true, false, false, false, false, false},
        { true, false, false, false, false, false},
        { true, false, false, false, false, false},
        { true, false, false, false, false, false},
    };
}
