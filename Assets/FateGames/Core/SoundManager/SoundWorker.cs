using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateGames.Core
{
    public class SoundWorker : FateMonoBehaviour
    {
        [SerializeField] private BoolVariable soundOn;
        [SerializeField] private WorkingSoundWorkerSet workingSet;
        [SerializeField] private AvailableSoundWorkerSet availableSet;
        private AudioSource audioSource = null;
        public bool Paused { get; private set; } = false;
        public bool Working { get; private set; } = false;

        public float TimeLeft { get => audioSource.clip.length - audioSource.time; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.mute = !soundOn.Value;
            /*
            Settings.OnSoundChange.AddListener((soundOn) => audioSource.mute = !soundOn);
            PauseButton.OnPause.AddListener(() => { if (Working) Pause(); });
            PauseButton.OnResume.AddListener(() => { if (Working) Unpause(); });*/
        }

        private void OnEnable()
        {
            availableSet.Add(this);
        }
        private void OnDisable()
        {
            availableSet.Remove(this);
            workingSet.Remove(this);
        }
        public void Initialize(AudioClip clip, float volume, float pitch, float spatialBlend, bool loop, Vector3 position, bool ignoreListenerPause)
        {
            if (Working) return;
            transform.position = position;
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.spatialBlend = spatialBlend;
            audioSource.loop = loop;
            audioSource.ignoreListenerPause = ignoreListenerPause;
        }

        public void Play()
        {
            if (Working) return;
            OnPlay();
            audioSource.Play();
            if (!audioSource.loop)
                Invoke(nameof(OnStop), TimeLeft);
        }

        public void OnPlay()
        {
            availableSet.Remove(this);
            workingSet.Add(this);
            Working = true;
        }

        public void OnStop()
        {
            StartCoroutine(OnStopRoutine());
        }

        private IEnumerator OnStopRoutine()
        {
            yield return new WaitUntil(() => !audioSource.isPlaying);
            workingSet.Remove(this);
            Working = false;
            Paused = false;
            availableSet.Add(this);
        }

        public void Stop()
        {
            if (!Working) return;
            audioSource.Stop();
            OnStop();
        }
        public void Pause()
        {
            if (!Working || Paused) return;
            audioSource.Pause();
            Paused = true;
            if (!audioSource.loop)
                CancelInvoke(nameof(OnStop));
        }
        public void Unpause()
        {
            if (!Working || !Paused) return;
            audioSource.UnPause();
            Paused = false;
            if (!audioSource.loop)
                Invoke(nameof(OnStop), TimeLeft);
        }
        public void Mute()
        {
            audioSource.mute = true;
        }
        public void Unmute()
        {
            audioSource.mute = false;
        }
    }
}
