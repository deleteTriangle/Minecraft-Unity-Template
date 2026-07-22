using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : ITickable
{
    private readonly WaveSpawner _spawner;
    private readonly WaveTimer _countdownTimer;
    private readonly WaveTimer _waveTimer;
    private readonly List<WaveConfig> _waves;

    private readonly HashSet<Enemy> _aliveEnemies = new();

    private int _currentWaveIndex = -1;
    private bool _allEnemiesSpawned;
    
    private Transform _playerTransform;

    public WaveState State { get; private set; } = WaveState.Idle;
    public int CurrentWaveNumber => _currentWaveIndex + 1;
    public int TotalWaves => _waves.Count;
    public float CountdownRemaining => _countdownTimer.Remaining;
    public float WaveTimeRemaining => _waveTimer.Remaining;

    public event Action<int> OnCountdownStarted;   // номер следующей волны
    public event Action<int> OnWaveStarted;         // номер волны
    public event Action<int> OnWaveCompleted;        // номер волны
    public event Action OnAllWavesCompleted;

    public WaveManager(List<WaveConfig> waves, WaveSpawner spawner, Transform playerTransform)
    {
        if (playerTransform == null) throw new ArgumentNullException(nameof(playerTransform), "Player transform is null");

        _waves = waves;
        _spawner = spawner;
        _playerTransform = playerTransform;
        
        _countdownTimer = new WaveTimer();
        _waveTimer = new WaveTimer();

        _countdownTimer.OnFinished += StartNextWave;
        _waveTimer.OnFinished += CompleteWave;

        _spawner.OnEnemySpawned += TrackEnemy;
    }

    public void StartCycle()
    {
        if (State != WaveState.Idle) return;
        if (_waves == null || _waves.Count == 0)
        {
            return;
        }
        BeginCountdown();
    }

    public void Tick(float deltaTime)
    {
        _countdownTimer.Tick(deltaTime);
        _waveTimer.Tick(deltaTime);
    }

    private void BeginCountdown()
    {
        State = WaveState.Countdown;
        var nextConfig = _waves[_currentWaveIndex + 1];

        _countdownTimer.Start(nextConfig.countdownDuration);
        OnCountdownStarted?.Invoke(CurrentWaveNumber + 1);
    }

    private void StartNextWave()
    {
        _currentWaveIndex++;
        _aliveEnemies.Clear();
        _allEnemiesSpawned = false;

        State = WaveState.InProgress;
        var config = _waves[_currentWaveIndex];

        _waveTimer.Start(config.waveDuration);
        OnWaveStarted?.Invoke(CurrentWaveNumber);

        _spawner.SpawnWave(config, () => _allEnemiesSpawned = true);
    }

    private void TrackEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        
        _aliveEnemies.Add(enemy);
        enemy.OnDied += () => OnEnemyDied(enemy);
        
        enemy.SetTarget(_playerTransform);
        enemy.Activate();
    }

    private void OnEnemyDied(Enemy enemy)
    {
        _aliveEnemies.Remove(enemy);
        CheckWaveCleared();
    }

    private void CheckWaveCleared()
    {
        if (_allEnemiesSpawned && _aliveEnemies.Count == 0)
            CompleteWave();
    }

    private void CompleteWave()
    {
        if (State != WaveState.InProgress) return;

        _waveTimer.Stop();
        State = WaveState.Completed;
        OnWaveCompleted?.Invoke(CurrentWaveNumber);

        bool hasMoreWaves = _currentWaveIndex + 1 < _waves.Count;

        if (hasMoreWaves)
        {
            State = WaveState.Idle;
            BeginCountdown();
        }
        else
        {
            OnAllWavesCompleted?.Invoke();
        }
    }
}