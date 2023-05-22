using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using FateGames.Core;

public class EffectPool : FateMonoBehaviour
{
    private ObjectPool<PooledEffect> pool;
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private int maxSize = 10;
    private Queue<PooledEffect> activeEffects = new();

    private void Awake()
    {
        pool = new(CreateEffect, OnGetEffect, OnReleaseEffect, maxSize: maxSize);
    }

    public PooledEffect Get()
    {
        if (pool.CountActive >= maxSize)
            pool.Release(activeEffects.Peek());
        return pool.Get();
    }

    public void Release(PooledEffect pooledEffect)
    {
        pool.Release(pooledEffect);
    }

    private PooledEffect CreateEffect()
    {
        GameObject obj = Instantiate(effectPrefab);
        PooledEffect pooledEffect = obj.GetComponent<PooledEffect>();
        if (!pooledEffect) pooledEffect = obj.AddComponent<PooledEffect>();
        pooledEffect.Release += () => { pool.Release(pooledEffect); };
        return pooledEffect;
    }

    public void OnGetEffect(PooledEffect pooledEffect)
    {
        Debug.Log("OnGetEffect", pooledEffect);
        activeEffects.Enqueue(pooledEffect);
        pooledEffect.OnObjectSpawn();
    }
    public void OnReleaseEffect(PooledEffect pooledEffect)
    {
        Debug.Log("OnReleaseEffect", pooledEffect);
        activeEffects.Dequeue();
        pooledEffect.OnRelease();
    }
}
