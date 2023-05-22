using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using UnityEngine.Events;

public class SkipButton : MonoBehaviour
{
    [SerializeField] private UnityEvent onSkip;
    public void Skip()
    {
        /*SDKManager.Instance.ShowRewardedAd(() =>
        {
            Debug.Log("Skip failed");

        }, () =>
        {
            onSkip.Invoke();
            GameManager.Instance.IncrementLevel();
            GameManager.Instance.LoadCurrentLevel();
            Debug.Log("Skip succeed");
        });*/
    }
}
