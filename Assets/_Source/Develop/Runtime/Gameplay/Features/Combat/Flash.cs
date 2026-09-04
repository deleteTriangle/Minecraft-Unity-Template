using System.Collections;
using UnityEngine;

public class Flash : MonoBehaviour
{
    [SerializeField] private float flashDuration = 0.2f;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _propertyBlock;

    public void Init()
    {
        _propertyBlock = new MaterialPropertyBlock();
        _renderers = GetComponentsInChildren<Renderer>();
    }

    public void Play()
    {
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetColor(Color.red);
        yield return new WaitForSeconds(flashDuration);
        SetColor(Color.white);
    }

    private void SetColor(Color color)
    {
        foreach (var r in _renderers)
        {
            r.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_Color", color);
            r.SetPropertyBlock(_propertyBlock);
        }
    }
}