using DG.Tweening;
using GameAnalyticsSDK;
using Lofelt.NiceVibrations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace FateGames.Core
{
    public class GameManager : Singleton<GameManager>
    {
        [Header("Properties")]
        [SerializeField] private GameStateVariable gameState;
        [Header("Target Frame Rate")]
        [SerializeField] private int defaultTargetFrameRate = -1;
        [Header("Save Management")]
        [SerializeField] private bool autoSave = false;
        [SerializeField] private float autoSavePeriod = 10;
        [SerializeField] private bool overrideSave = false;
        [SerializeField] private SaveDataVariable saveData, overrideSaveData;
        [Header("Scene Management")]
        [SerializeField] private int firstLevelSceneIndex = 1;
        [SerializeField] private bool loop;
        [SerializeField] private GameObject loadingScreen;
        [Header("Level Management")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private GameObject loseScreen, winScreen;
        [Header("Sound Management")]
        [SerializeField] private BoolVariable soundOn;
        [SerializeField] private GameObject soundWorkerPrefab;
        [SerializeField] private WorkingSoundWorkerSet workingWorkerSet;
        [SerializeField] private AvailableSoundWorkerSet availableWorkerSet;
        [Header("Haptic Management")]
        [SerializeField] private BoolVariable vibrationOn;
        [Header("Events")]
        [SerializeField] private UnityEvent onPause;
        [SerializeField] private UnityEvent onResume, onLevelStarted, onLevelWon, onLevelFailed, onLevelCompleted;
        private GamePauser gamePauser;
        private SaveManager saveManager;
        private WaitForSeconds waitForAutoSavePeriod;
        private SceneManager sceneManager;
        private LevelManager levelManager;
        private SoundManager soundManager;
        private HapticManager hapticManager;

        private float lastTotalPlaytimeSaveTime = 0;
        public float totalPlaytime { get => PlayerPrefs.GetFloat("TotalPlaytime", 0); private set => PlayerPrefs.SetFloat("TotalPlaytime", value); }

        protected override void Awake()
        {
            base.Awake();
            if (duplicated) return;
            Initialize();
            
        }

        private void Start()
        {
            if (autoSave && !overrideSave) StartCoroutine(AutoSaveRoutine());

        }

        public void OnSDKManagerInitialized()
        {
            if (!sceneManager.IsLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene()))
                sceneManager.LoadCurrentLevel();
        }

        public void Initialize()
        {
            SetTargetFrameRate(defaultTargetFrameRate);
            InitializeGamePauser();
            InitializeSaveManagement();
            InitializeSceneManagement();
            InitializeLevelManagement();
            InitializeSoundManagement();
            InitializeHapticManagement();
        }
        private void InitializeGamePauser()
        {
            gamePauser = new(onPause, onResume, gameState);
        }
        private void InitializeSaveManagement()
        {
            saveManager = new(saveData, overrideSaveData);
            saveManager.Load(overrideSave);
            waitForAutoSavePeriod = new(autoSavePeriod);
        }
        private void InitializeSceneManagement()
        {
            sceneManager = new(gameState, firstLevelSceneIndex, loop, saveData, loadingScreen);
        }
        private void InitializeLevelManagement()
        {
            levelManager = new(loseScreen, winScreen, gameState, onLevelStarted, onLevelCompleted, onLevelFailed, onLevelWon);
        }
        private void InitializeSoundManagement()
        {
            soundManager = new(gameState, soundOn, soundWorkerPrefab, workingWorkerSet, availableWorkerSet);
        }
        private void InitializeHapticManagement()
        {
            hapticManager = new(vibrationOn);
        }

        public void StartLevel()
        {
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "Level_Progress", saveData.Value.Level);
            levelManager.StartLevel();
        }

        public void FinishLevel(bool success)
        {
            GameAnalytics.NewProgressionEvent(success? GAProgressionStatus.Complete: GAProgressionStatus.Fail, "Level_Progress", saveData.Value.Level);
            DOTween.KillAll();
            levelManager.FinishLevel(success);
            saveData.Value.Level++;
            SaveToDevice();
        }

        public void SkipLevel()
        {
            saveData.Value.Level++;
            SaveToDevice();
            LoadCurrentLevel();
        }

        public void LoadCurrentLevel()
        {
            sceneManager.LoadCurrentLevel();
        }

        private IEnumerator AutoSaveRoutine()
        {
            yield return waitForAutoSavePeriod;
            SaveToDevice();
            yield return AutoSaveRoutine();
        }

        public void SaveToDevice()
        {
            if (overrideSave) return;
            saveManager.SaveToDevice(saveData.Value);
            PlayerPrefs.SetFloat("TotalPlaytime", Time.time - lastTotalPlaytimeSaveTime + totalPlaytime);
            lastTotalPlaytimeSaveTime = Time.time;
        }

        public void SetTargetFrameRate(int targetFrameRate) => Application.targetFrameRate = targetFrameRate;

        public void PauseGame()
        {
            gamePauser.PauseGame();
        }
        public void ResumeGame()
        {
            gamePauser.ResumeGame();
        }

        public void PlaySoundOneShot(SoundEntity entity)
        {
            PlaySound(entity);
        }

        public void PlaySoundOneShotIgnorePause(SoundEntity entity)
        {
            PlaySound(entity, true);
        }

        public SoundWorker PlaySound(SoundEntity entity, bool ignoreListenerPause = false)
        {
            return PlaySound(entity, Vector3.zero, ignoreListenerPause);
        }

        public SoundWorker PlaySound(SoundEntity entity, Vector3 position, bool ignoreListenerPause = false, bool pauseOnStartIfGamePaused = false)
        {
            return soundManager.PlaySound(entity, position, ignoreListenerPause, pauseOnStartIfGamePaused);
        }

        public void PlayHaptic()
        {
            hapticManager.PlayHaptic();
        }
        public void PlayHaptic(HapticPatterns.PresetType presetType)
        {
            hapticManager.PlayHaptic(presetType);
        }

        void OnEnable()
        {
            //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        void OnDisable()
        {
            //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            if (sceneManager.IsLevel(scene))
            {
                gameState.Value = GameState.BEFORE_START;
                SDKManager.Instance.ShowInterstitial();
                if (autoStart)
                    StartLevel();
            }
        }


    }
}
