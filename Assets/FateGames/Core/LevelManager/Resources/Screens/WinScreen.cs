using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
public class WinScreen : MonoBehaviour
{
    [SerializeField] private SoundEntity sound;
    void Start()
    {
        GameManager.Instance.PlaySound(sound);
    }


}
