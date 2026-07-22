using System;
using UnityEngine;

[Serializable]
public class SpawnGroup
{
    public EnemyConfig config;
    public int count = 1;
    public float delayBeforeSpawn = 0f; // задержка перед спавном этой группы
}