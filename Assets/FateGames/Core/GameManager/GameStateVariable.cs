using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateGames.Core
{
    [CreateAssetMenu(menuName = "Fate/Variable/GameState Variable")]
    public class GameStateVariable : Variable<GameState>
    {
        private void OnEnable()
        {
            Value = GameState.NONE;
            OnValueChanged.RemoveAllListeners();
            OnValueChanged.AddListener(PrintTransition);
        }
        public void PrintTransition(GameState previous, GameState current)
        {
            Debug.Log(previous + " => " + current, this);
        }
    }

    public enum GameState { NONE, LOADING, BEFORE_START, IN_GAME, LOSE_SCREEN, WIN_SCREEN, PAUSED, BOOTING }

}