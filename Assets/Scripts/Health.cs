using UnityEngine;

// 1. Add the IDamageable here
public class Health : MonoBehaviour, IDamagable 
{
    public int maxHealth = 1;
    public int currentHealth;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    // 2. Added this NEW method required by the IDamageable interface
    public void TakeDamage(float damageAmount)
    {
        
        TakeDamage(Mathf.RoundToInt(damageAmount));
    }

    // 3. Keep your existing logic exactly the same for the rest
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log(gameObject.name + " died.");
        if (gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoseReincarnation();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    private void OnEnable()
    {
        GameManager.OnReincarnationLost += ResetHealth;
    }

    private void OnDisable()
    {
        GameManager.OnReincarnationLost -= ResetHealth;
    }

    // Context menu for testing
    [ContextMenu("Kill")]
    public void Kill()
    {
        TakeDamage(maxHealth);
    }
}