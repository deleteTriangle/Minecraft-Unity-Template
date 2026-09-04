using System;

public class WaveTimer
{
    private float _duration;
    private float _elapsed;
    private bool _running;

    public float Remaining => MathF.Max(_duration - _elapsed, 0f);
    public bool IsFinished => _elapsed >= _duration;

    public event Action OnFinished;

    public void Start(float duration)
    {
        _duration = duration;
        _elapsed = 0f;
        _running = true;
    }

    public void Stop() => _running = false;

    public void Tick(float deltaTime)
    {
        if (!_running || IsFinished) return;

        _elapsed += deltaTime;

        if (IsFinished)
        {
            _running = false;
            OnFinished?.Invoke();
        }
    }
}