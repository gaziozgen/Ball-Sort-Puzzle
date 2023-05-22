using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using System;

public class PooledEffect : FateMonoBehaviour, IPooledObject
{
    [SerializeField] private ParticleSystem effect;
    private WaitForSeconds waitForEffectDuration;
    public Action Release { get; set; }
    private IEnumerator releaseRoutine;

    private void Awake()
    {
        InitializeEffect();
    }

    private void InitializeEffect()
    {
        if (!effect) effect = GetComponentInChildren<ParticleSystem>(true);
        if (!effect) return;
        ParticleSystem.MainModule main = effect.main;
        main.playOnAwake = false;
        waitForEffectDuration = new(main.duration);
    }

    private void PlayEffect()
    {
        if (!effect)
        {
            Debug.LogError("There is no particle system to play!", this);
            return;
        }
        Activate();
        effect.Play();
    }
    private void StopEffect()
    {
        if (!effect)
        {
            Debug.LogError("There is no particle system to stop!", this);
            return;
        }
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Deactivate();
    }

    public void OnObjectSpawn()
    {
        PlayEffect();
        StartReleaseCoroutine();
    }

    private void StartReleaseCoroutine()
    {
        releaseRoutine = ReleaseRoutine();
        StartCoroutine(releaseRoutine);
    }

    private void StopReleaseCoroutine()
    {
        if (releaseRoutine == null) return;
        StopCoroutine(releaseRoutine);
        releaseRoutine = null;
    }

    private IEnumerator ReleaseRoutine()
    {
        yield return waitForEffectDuration;
        Release();
        releaseRoutine = null;
    }

    public void OnRelease()
    {
        StopEffect();
        Deactivate();
        StopReleaseCoroutine();
    }

    private void Reset()
    {
        InitializeEffect();
    }
}
