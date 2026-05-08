using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Health : MonoBehaviour, IDamageable
{
    [Min(1)]
    public int maxHealth = 1;

    [Min(1)]
    public int currentHealth = 1;

    public event Action<DamageInfo> DamageTaken;
    public event Action<DamageInfo> Death;
    public event Action<int, int> HealthChanged;

    private bool isDead;

    private void Awake()
    {
        NormalizeHealth();
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

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    [ContextMenu("Kill")]
    public void Kill()
    {
        TakeDamage(DamageInfo.FromAmount(currentHealth));
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
        currentHealth = Mathf.Clamp(currentHealth, 1, maxHealth);
    }
}