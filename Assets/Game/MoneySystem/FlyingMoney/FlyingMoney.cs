using FateGames.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using DG.Tweening;

public class FlyingMoney : FateMonoBehaviour, IPooledObject
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform imageTransform = null;
    [SerializeField] private SoundEntity coinSound = null;

    public Action Release { get; set; }

    public void OnObjectSpawn()
    {
        canvas.enabled = true;
    }

    public void OnRelease()
    {
        canvas.enabled = false;
    }

    public void DirectGoToUI(int amount, Vector3 startPositon)
    {
        float duration = 0.6f;
        float startSize = 0.5f;
        float endSize = 1f;

        Vector3 target = MoneyUI.Instance.MoneyTarget;
        imageTransform.position = startPositon;
        imageTransform.localScale = Vector3.one * startSize;
        imageTransform.DOScale(Vector3.one * endSize, duration).SetEase(Ease.InCubic);
        DOTween.To(() => imageTransform.position, (x) => imageTransform.position = x, target, duration).SetEase(Ease.InCubic).OnComplete(() =>
        {
            MoneyUI.Instance.AddMoney(amount);
            GameManager.Instance.PlaySound(coinSound);
            Release();
        });
    }

    public void GoUIWithBurstMove(int amount, Vector2 startPositon, Vector2 midPosition)
    {
        imageTransform.position = startPositon;
        imageTransform.localScale = Vector3.one;
        imageTransform.DOMove(midPosition, 0.5f).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            Vector3 target = MoneyUI.Instance.MoneyTarget;
            float randomTimeRange = 1f + UnityEngine.Random.Range(-0.3f, 0.3f);
            DOTween.To(() => imageTransform.position, (x) => imageTransform.position = x, target, randomTimeRange).SetEase(Ease.InCubic).OnComplete(() =>
            {
                MoneyUI.Instance.AddMoney(amount);
                GameManager.Instance.PlaySound(coinSound);
                Release();
            });
        });
    }
}
