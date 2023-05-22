using FateGames.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainButtons : MonoBehaviour
{
    [SerializeField] private int addTubeCost = 500;
    [SerializeField] private int undoCountPerLevel = 4;
    [SerializeField] private TextMeshProUGUI UndoText;
    [SerializeField] private GameObject panel = null;
    [SerializeField] private GameObject undoAdImage = null;
    [SerializeField] private TextMeshProUGUI tubeCostText = null;
    [SerializeField] private Button buyTubeButton = null;
    [SerializeField] private Button addTubeButton = null;
    [SerializeField] private SoundEntity sound;

    private int leftUndoCount;

    private void Awake()
    {
        leftUndoCount = undoCountPerLevel;
        tubeCostText.text = "Cost " + addTubeCost.ToString();
    }

    public void Replay()
    {
        GameManager.Instance.PlayHaptic();
        GameManager.Instance.PlaySound(sound);
        GameArea.Instance.UndoAll();
        leftUndoCount = undoCountPerLevel;
        UndoText.text = leftUndoCount.ToString();
        undoAdImage.SetActive(false);

        SDKManager.Instance.ShowInterstitial();
    }

    public void Undo()
    {
        GameManager.Instance.PlayHaptic();
        GameManager.Instance.PlaySound(sound);
        if (leftUndoCount > 0)
        {
            if (GameArea.Instance.Undo())
            {
                leftUndoCount--;
                UndoText.text = leftUndoCount.ToString();

                if (leftUndoCount == 0)
                {
                    undoAdImage.SetActive(true);
                }
            }
        }
        else
        {
            SDKManager.Instance.ShowRewardedAd(() => { }, () =>
            {
                leftUndoCount++;
                UndoText.text = leftUndoCount.ToString();
                undoAdImage.SetActive(false);
            });
        }
    }

    public void Skip()
    {
        GameManager.Instance.PlayHaptic();
        GameManager.Instance.PlaySound(sound);
        SDKManager.Instance.ShowRewardedAd(() => { }, () => GameManager.Instance.SkipLevel());
    }

    public void OpenAddTubePanel()
    {
        GameManager.Instance.PlayHaptic();
        GameManager.Instance.PlaySound(sound);
        panel.SetActive(true);
        if (MoneyUI.Instance.Money >= addTubeCost) buyTubeButton.interactable = true;
        else buyTubeButton.interactable = false;
    }

    public void ClosePanel()
    {
        GameManager.Instance.PlayHaptic();
        GameManager.Instance.PlaySound(sound);
        panel.SetActive(false);
    }

    public void AddTubeByMoney()
    {
        GameManager.Instance.PlayHaptic();
        GameManager.Instance.PlaySound(sound);
        MoneyUI.Instance.SpendMoney(addTubeCost);
        GameArea.Instance.AddTube();
        addTubeButton.interactable = false;
        ClosePanel();
    }

    public void AddTubeByAd()
    {
        GameManager.Instance.PlayHaptic();
        GameManager.Instance.PlaySound(sound);
        SDKManager.Instance.ShowRewardedAd(() => { }, () =>
        {
            GameArea.Instance.AddTube();
            addTubeButton.interactable = false;
            ClosePanel();
        });
        
    }
}
