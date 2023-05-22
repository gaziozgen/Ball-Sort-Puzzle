using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using DG.Tweening;
public class AnimatedUIElement : FateMonoBehaviour
{
    [SerializeField] private Transform container;

    public void Show()
    {
        container.gameObject.SetActive(true);
        container.localScale = Vector3.zero;
        container.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    public void Hide()
    {

        container.localScale = Vector3.one;
        container.DOScale(Vector3.zero, 0.3f).OnComplete(() => {
            container.gameObject.SetActive(false);
        });
    }

    public void HideWithoutAnimation()
    {
        container.gameObject.SetActive(false);
    }

    public void ShowWithoutAnimation()
    {
        container.gameObject.SetActive(true);
    }

    private void Reset()
    {
        container = transform;
    }

}
