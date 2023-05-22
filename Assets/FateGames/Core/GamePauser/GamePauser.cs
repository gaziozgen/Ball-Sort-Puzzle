using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FateGames.Core
{
    public class GamePauser
    {
        private UnityEvent onPause, onResume;
        private GameState gameStateBeforePause;
        private GameStateVariable gameState;

        public GamePauser(UnityEvent onPause, UnityEvent onResume, GameStateVariable gameState)
        {
            this.onPause = onPause;
            this.onResume = onResume;
            this.gameState = gameState;
        }

        public void PauseGame()
        {
            if (gameState.Value == GameState.PAUSED) return;
            gameStateBeforePause = gameState.Value;
            gameState.Value = GameState.PAUSED;
            onPause.Invoke();
        }

        public void ResumeGame()
        {
            if (gameState.Value != GameState.PAUSED) return;
            gameState.Value = gameStateBeforePause;
            onResume.Invoke();
        }
    }

}
