using FateGames.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

public class FateObjectPool<T> where T : FateMonoBehaviour, IPooledObject
{
    private GameObject prefab; 
    private IObjectPool<T> pool;
    private Transform poolParent;

    public FateObjectPool(GameObject prefab, bool collectionCheck, int defaultCapacity, int maxSize)
    {
        this.prefab = prefab;
        pool = new UnityEngine.Pool.ObjectPool<T>(CreateObject, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, collectionCheck, defaultCapacity, maxSize);
        poolParent = new GameObject(typeof(T) + " Pool").transform;
    }

    public T Get()
    {
        return pool.Get();
    }

    public T Get(Vector3 position)
    {
        T obj = pool.Get();
        obj.transform.position = position;
        return obj;
    }

    public T Get(Vector3 position, Quaternion rotation)
    {
        T obj = pool.Get();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }

    private T CreateObject()
    {
        T obj = Object.Instantiate(prefab).GetComponent<T>();
        obj.Release += () => pool.Release(obj);
        return obj;
    }

    private void OnReleaseToPool(T obj)
    {
        obj.OnRelease();
        obj.transform.SetParent(poolParent);
        obj.Deactivate();

    }

    private void OnGetFromPool(T obj)
    {
        obj.Activate();
        obj.transform.SetParent(null);
        obj.OnObjectSpawn();
    }

    private void OnDestroyPooledObject(T obj)
    {
        Object.Destroy(obj.gameObject);
    }
}
