using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    private readonly List<ITickable> _tickables = new();
    
    public void Register(ITickable tickable) => _tickables.Add(tickable);
    public void Unregister(ITickable tickable) => _tickables.Remove(tickable);

    private void Update()
    {
        foreach (var tickable in _tickables)
            tickable.Tick(Time.deltaTime);
    }
}

public interface ITickable
{
    void Tick(float deltaTime);
}
