using UnityEngine;

public interface IMovable
{
    void MoveTo(Vector3 destination);
    void Stop();
}