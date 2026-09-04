using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public PlayerStatsConfig config;
    
    public void Init()
    {
        if (config == null)
            throw new System.ArgumentNullException(nameof(config), $"PlayerStatsConfig is null on {name}");
    }
}
