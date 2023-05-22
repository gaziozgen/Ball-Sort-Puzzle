using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
namespace FateGames.Core
{

    public class LevelManager
    {
        private GameObject loseScreen, winScreen;
        private GameStateVariable gameState;
        private UnityEvent OnLevelStarted, OnLevelCompleted, OnLevelFailed, OnLevelWon;

        public LevelManager(GameObject loseScreen, GameObject winScreen, GameStateVariable gameState, UnityEvent onLevelStarted, UnityEvent onLevelCompleted, UnityEvent onLevelFailed, UnityEvent onLevelWon)
        {
            this.loseScreen = loseScreen;
            this.winScreen = winScreen;
            this.gameState = gameState;
            OnLevelStarted = onLevelStarted;
            OnLevelCompleted = onLevelCompleted;
            OnLevelFailed = onLevelFailed;
            OnLevelWon = onLevelWon;
        }

        public void StartLevel()
        {
            gameState.Value = GameState.IN_GAME;
            OnLevelStarted.Invoke();
        }

        public void FinishLevel(bool success)
        {
            if (success)
            {
                Object.Instantiate(winScreen);
                gameState.Value = GameState.WIN_SCREEN;
                OnLevelWon.Invoke();
            }
            else
            {
                Object.Instantiate(loseScreen);
                gameState.Value = GameState.LOSE_SCREEN;
                OnLevelFailed.Invoke();
            }
            OnLevelCompleted.Invoke();
        }

#if UNITY_EDITOR

        [MenuItem("Fate/Level/Open Win Screen")]
        public static void OpenWinScreen()
        {
            AssetDatabase.OpenAsset(Resources.Load("Screens/WinScreen"));
        }

        [MenuItem("Fate/Level/Open Lose Screen")]
        public static void OpenLoseScreen()
        {
            AssetDatabase.OpenAsset(Resources.Load("Screens/LoseScreen"));
        }
#endif
    }

}

