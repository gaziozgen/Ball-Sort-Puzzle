using FateGames.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Ball : FateMonoBehaviour
{

    [SerializeField] private Ball_Type ball_type;
    [SerializeField] private GameObject[] ballTypes;
    [SerializeField] private Animator animator = null;


    private void Awake()
    {
        UpdateColor();
    }

    public Ball_Type GetColor()
    {
        return ball_type;
    }

    public void SetColor(Ball_Type color)
    {
        ball_type = color;
        UpdateColor();
    }

    public void BounceFromTop()
    {
        animator.SetTrigger("TopBounce");
    }

    public void BounceFromBot()
    {
        animator.SetTrigger("BotBounce");
    }

    private void UpdateColor()
    {
        for (int i = 0; i < ballTypes.Length; i++)
        {
            if (i == (int)ball_type) ballTypes[i].SetActive(true);
            else ballTypes[i].SetActive(false);
        }
    }
}

public enum Ball_Type { Color1, Color2, Color3, Color4, Color5, Color6, Color7, Color8, Color9 }
