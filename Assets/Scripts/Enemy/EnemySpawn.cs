
using System.Collections;
using UnityEngine;

public struct WaveInfo
{
    public int wave;
    public int[] enemySpawnTable;

    public WaveInfo(int wave, int[] enemySpawnTable)
    {
        this.wave = wave;
        this.enemySpawnTable = enemySpawnTable;
    }
}


[RequireComponent(typeof(EnemyPool))]
public class EnemySpawn : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 0.5f;

    [SerializeField] private float spawnRate = 10f;

    public System.Action<WaveInfo> OnWaveClear;
    public System.Action OnGameClear;

    private EnemyPool pool;

    // 현재 wave
    public int currentWave = 1;

    // 스폰 위치(원)
    public float spawnDistance = 50f;

    private bool isGameOn;

    public WaveInfo[] WaveInfos => waveInfos;



    private void Awake()
    {
        pool = GetComponent<EnemyPool>();
    }

    void Start()
    {
        GameStart();
    }

    public void Initialize(Transform target)
    {
        pool.Initialize(target);
    }
    
    IEnumerator SpawnEnemy()
    {
        int waveIndex = 0;
        int tableIndex = 0;

        while (isGameOn)
        {
            if (waveIndex >= waveInfos.Length)
            {
                // 웨이브 다끝남.

                break;
            }
            var waveData = waveInfos[waveIndex];

            if (tableIndex >= waveData.enemySpawnTable.Length)
            {
                //다음 웨이브 넘겨
                waveIndex++;
                tableIndex = 0;
                if (waveIndex < waveInfos.Length)
                {
                    OnWaveClear?.Invoke(waveInfos[waveIndex]);
                }
                    
                yield return new WaitForSeconds(spawnRate);
                continue;
            }
            var spawnArr = waveData.enemySpawnTable;
            // 스폰 처리
            var enemyObj = pool.Pop(spawnArr[tableIndex]);
            enemyObj.gameObject.SetActive(true);
            enemyObj.transform.position = RandomPosition();
            UnityEngine.Debug.Log($"EnemySpawn!! Name : {enemyObj.name}, Wave : {waveData.wave}, TableIndex : {tableIndex}, SpawnIndex : {spawnArr[tableIndex]}");
            // 스폰 인터벌 대기
            yield return new WaitForSeconds(spawnInterval);
            tableIndex++;
        }

        while (!pool.IsAllInactive())
        {
            yield return null;
        }
        Debug.Log("Game Clear!!");
        OnGameClear?.Invoke();

    }

    private Vector3 RandomPosition()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        return new Vector3(randomDirection.x, 0f, randomDirection.y) * spawnDistance + new Vector3(0, 10, 0);
    }

    public void GameStart()
    {
        isGameOn = true;
        StartCoroutine(SpawnEnemy());
    }

    public void GameOver()
    {
        isGameOn = false;
    }

    private WaveInfo[] waveInfos = new WaveInfo[]
    {
        new WaveInfo(1,new int[] { 0 }),
        new WaveInfo(2,new int[] { 0 }),
        new WaveInfo(3,new int[] { 0, 0 }),
        new WaveInfo(4,new int[] { 0, 0, 0}),
        new WaveInfo(4,new int[] { 0, 0, 0, 0, 0}),
    };
}
