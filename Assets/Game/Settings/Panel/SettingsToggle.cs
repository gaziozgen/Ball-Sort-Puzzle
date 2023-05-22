using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using UnityEngine.UI;
using UnityEngine.Events;

public class SettingsToggle : MonoBehaviour
{
    [SerializeField] private BoolVariable variable;
    [SerializeField] private Sprite offSprite, onSprite;
    [SerializeField] private Image image;
    [SerializeField] private UnityEvent onToggled, onTurnedOn, onTurnedOff;

    private void Start()
    {
        SetImage();
    }

    public void SetImage()
    {
        image.sprite = variable.Value ? onSprite : offSprite;
    }

    public void Toggle()
    {
        GameManager.Instance.PlayHaptic();
        variable.Value = !variable.Value;
        SetImage();
        onToggled.Invoke();
        if (variable.Value) onTurnedOn.Invoke();
        else onTurnedOff.Invoke();
    }

}
