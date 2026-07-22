using UnityEngine;

public class Player : Entity
{
    public void Init(EntityConfig config)
    {
        base.Init(config);
        OnDied += OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        Debug.Log("Player died");
    }
}