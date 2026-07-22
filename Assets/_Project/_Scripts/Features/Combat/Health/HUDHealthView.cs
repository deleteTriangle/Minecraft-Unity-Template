using TMPro;
using UnityEngine;

public class HUDHealthView : MonoBehaviour
{
    private Health health;
    
    public TMP_Text amountText;

    public void Init(Health health)
    {
        this.health = health;

        UpdateView(health.CurrentHealth);
        health.OnHealthChanged += UpdateView;
    }

    private void UpdateView(int healthAmount)
    {
        amountText.text = healthAmount.ToString();
    }
}
