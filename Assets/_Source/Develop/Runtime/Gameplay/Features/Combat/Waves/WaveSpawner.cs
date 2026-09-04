using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public event Action<Enemy> OnEnemySpawned;

    private Transform[] _spawnPoints;

    public void Init(Transform[] spawnPoints)
    {
        _spawnPoints = spawnPoints;
    }

    public void SpawnWave(WaveConfig config, Action onAllSpawned)
    {
        StartCoroutine(SpawnRoutine(config, onAllSpawned));
    }

    private IEnumerator SpawnRoutine(WaveConfig config, Action onAllSpawned)
    {
        foreach (var group in config.spawnGroups)
        {
            yield return new WaitForSeconds(group.delayBeforeSpawn);

            for (int i = 0; i < group.count; i++)
            {
                Enemy enemy = SpawnEnemy(group.config.prefab);
                enemy.Init(group.config);
                OnEnemySpawned?.Invoke(enemy);
            }
        }

        onAllSpawned?.Invoke();
    }

    private Enemy SpawnEnemy(Enemy prefab)
    {
        var point = GetRandomSpawnPoint();
        var enemy = Instantiate(prefab, point.position, point.rotation);
        return enemy;
    }

    private Transform GetRandomSpawnPoint()
    {
        return _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];
    }
}