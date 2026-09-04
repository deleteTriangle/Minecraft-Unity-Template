using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Combat/WaveConfig")]
public class WaveConfig : ScriptableObject
{
    public float countdownDuration = 5f;  // отсчёт перед волной
    public float waveDuration = 30f;      // максимальное время волны
    public List<SpawnGroup> spawnGroups = new();
}