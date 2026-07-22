using UnityEngine;

public class BlockDestructionProgress : MonoBehaviour
{
    [SerializeField] private Material destructionMaterial;
    [SerializeField] private float minScale = 0.3f;
    [SerializeField] private float maxScale = 1.2f;
    
    private MaterialPropertyBlock propertyBlock;
    private Renderer blockRenderer;
    private int destructionProgressID;
    private float currentProgress = 0f;
    
    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        blockRenderer = GetComponent<Renderer>();
        
        // ID свойства в шейдере
        destructionProgressID = Shader.PropertyToID("_DestructionProgress");
        
        // Изначально скрываем
        enabled = false;
    }
    
    public void SetProgress(float progress)
    {
        if (!enabled) enabled = true;
        
        currentProgress = Mathf.Clamp01(progress);
        
        if (blockRenderer != null)
        {
            blockRenderer.GetPropertyBlock(propertyBlock);
            
            // Передаем прогресс в шейдер
            propertyBlock.SetFloat(destructionProgressID, currentProgress);
            
            // Также можно масштабировать сам объект (опционально)
            // transform.localScale = Vector3.one * Mathf.Lerp(minScale, maxScale, currentProgress);
            
            blockRenderer.SetPropertyBlock(propertyBlock);
        }
    }
    
    public void Hide()
    {
        enabled = false;
        
        if (blockRenderer != null)
        {
            blockRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(destructionProgressID, 0f);
            blockRenderer.SetPropertyBlock(propertyBlock);
        }
        
        // Возвращаем нормальный масштаб
        // transform.localScale = Vector3.one;
    }
}