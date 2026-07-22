using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public EntityConfig playerConfig;

    public Player player;
    
    [SerializeField] private List<WaveConfig> waves;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private Transform[] spawnPoints;

    private WaveManager waveManager;
    private CombatSystem combatSystem;

    public void Init()
    {
        player = G.Player;
        combatSystem = new CombatSystem(this);
        
        waveSpawner.Init(spawnPoints);
        waveManager = new WaveManager(waves, waveSpawner, player.transform);
        G.GameLoop.Register(waveManager);

        waveSpawner.OnEnemySpawned += enemy =>
        {
            G.GameLoop.Register(enemy);
        };

        SubscribeToWaveEvents();

        waveManager.StartCycle();
    }
    
    private void SubscribeToWaveEvents()
    {
        waveManager.OnCountdownStarted += wave =>
            Debug.Log($"Wave {wave} starts in {waves[wave - 1].countdownDuration}s");

        waveManager.OnWaveStarted += wave =>
            Debug.Log($"Wave {wave} started!");

        waveManager.OnWaveCompleted += wave =>
            Debug.Log($"Wave {wave} completed!");

        waveManager.OnAllWavesCompleted += () =>
            Debug.Log("All waves completed! You win!");
    }
}
