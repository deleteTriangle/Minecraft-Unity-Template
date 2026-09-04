using System;

public class Health
{
    public event Action<int> OnHealthChanged;
    public event Action OnDeath;
    
    private int maxHealth;
    private int currentHealth;
    
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    public Health(int maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage >= 0)
        {
            currentHealth -= damage;
            currentHealth = Math.Max(currentHealth, 0);
            
            OnHealthChanged?.Invoke(currentHealth);
            if (currentHealth <= 0) OnDeath?.Invoke();
        }
    }

    public void Heal(int heal)
    {
        if (heal >= 0)
        {
            currentHealth += heal;
            currentHealth = Math.Min(currentHealth, maxHealth);
            
            OnHealthChanged?.Invoke(currentHealth);
        }
    }
}
