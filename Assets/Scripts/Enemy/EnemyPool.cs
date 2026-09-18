using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private List<EnemyBase> enemyPrefabs;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private Transform target;

    private List<List<EnemyBase>> pools;

    

    void Awake()
    {
        Initialize(target);
    }

    public void Initialize(Transform target)
    {
        this.target = target;

        pools = new List<List<EnemyBase>>();

        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            List<EnemyBase> pool = new List<EnemyBase>();

            for (int j = 0; j < poolSize; j++)
            {
                EnemyBase enemy = SpawnEnemy(enemyPrefabs[i]);
                pool.Add(enemy);
            }

            pools.Add(pool);
        }
    }

    private EnemyBase SpawnEnemy(EnemyBase enemyPrefab)
    {
        EnemyBase enemy = Instantiate(enemyPrefab, transform);

        enemy.SetData(target);
        enemy.gameObject.SetActive(false);

        return enemy;
    }

    public EnemyBase Pop(int index)
    {
        List<EnemyBase> pool = pools[index];

        foreach (EnemyBase enemy in pool)
        {
            if (!enemy.gameObject.activeSelf)
            {
                // 필요하다면 꺼낼 때 다시 target 갱신
                enemy.SetData(target);

                return enemy;
            }
        }

        EnemyBase newbie = SpawnEnemy(enemyPrefabs[index]);

        pool.Add(newbie);

        return newbie;
    }

    public bool IsAllInactive()
    {
        foreach (List<EnemyBase> pool in pools)
        {
            foreach (EnemyBase enemy in pool)
            {
                if (enemy.gameObject.activeSelf)
                    return false;
            }
        }

        return true;
    }
}