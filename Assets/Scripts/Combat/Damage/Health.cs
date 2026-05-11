using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1)]
    private int maxHealth = 1;

    [SerializeField, Min(0)]
    private int currentHealth = 1;

    public event Action<DamageInfo> DamageTaken;
    public event Action<DamageInfo> Death;
    public event Action<int, int> HealthChanged;

    private bool isDead;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        NormalizeHealth();
    }

    private void Start()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnValidate()
    {
        NormalizeHealth();
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (isDead || damageInfo.Amount <= 0)
        {
            return;
        }

        if (IsDamageBlocked(damageInfo))
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damageInfo.Amount);
        DamageTaken?.Invoke(damageInfo);
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die(damageInfo);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        TakeDamage(DamageInfo.FromAmount(Mathf.RoundToInt(damageAmount)));
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    [ContextMenu("Kill")]
    public void Kill()
    {
        if (isDead) return;
        currentHealth = 0;
        HealthChanged?.Invoke(currentHealth, maxHealth);
        Die(DamageInfo.FromAmount(maxHealth));
    }

    private bool IsDamageBlocked(DamageInfo damageInfo)
    {
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IDamageBlocker damageBlocker && damageBlocker.BlocksDamage(damageInfo))
            {
                return true;
            }
        }

        return false;
    }

    private void Die(DamageInfo damageInfo)
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        Death?.Invoke(damageInfo);
    }

    private void NormalizeHealth()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
}
