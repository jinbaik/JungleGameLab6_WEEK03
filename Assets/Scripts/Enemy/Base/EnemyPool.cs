using System;
using System.Collections.Generic;
using UnityEngine;


public class EnemyPool : MonoBehaviour
{
    [SerializeField] private List<EnemyBase> enemyPrefabs;
    [SerializeField] private int poolSize;

    private List<List<GameObject>> pools;

    private Transform target;

    public void Initialize(Transform target)
    {
        this.target = target;

        pools = new List<List<GameObject>>();
        foreach (var prefab in enemyPrefabs)
        {
            var pool = new List<GameObject>();

            for (int i = 0; i < poolSize; i++)
            {
                pool.Add(SpawnEnemy(prefab));
            }
            pools.Add(pool);
        }
    }

    private GameObject SpawnEnemy(EnemyBase enemyBase)
    {
        var prefab = Instantiate(enemyBase.gameObject);
        prefab.SetActive(false);
        return prefab;
    }

    public GameObject Pop(int index)
    {
        if (index >= pools.Count)
            return null;
        var selectedPool = pools[index];
        foreach (var item in selectedPool)
        {
            if (!item.activeInHierarchy)
                return item;
        }

        var newbie = SpawnEnemy(enemyPrefabs[index]);
        newbie.SetActive(false);
        selectedPool.Add(newbie);
        return newbie;
    }

    public GameObject GetObject()
    {
        return Pop(0);
    }

    public GameObject GetObject2()
    {
        return Pop(1);
    }

    public bool IsAllInactive()
    {

        foreach (var pool in pools)
        {
            foreach (var item in pool)
            {
                if (item.activeInHierarchy)
                    return false;
            }
        }

        return true;

    }
}
