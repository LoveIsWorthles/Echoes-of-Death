using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    private Health health;
    private EnemyGuardAI ai;

    private void Awake()
    {
        health = GetComponent<Health>();
        ai = GetComponent<EnemyGuardAI>();
    }

    private void OnEnable() => health.Death += HandleDeath;
    private void OnDisable() => health.Death -= HandleDeath;

    private void HandleDeath(DamageInfo info)
    {
        // Disable AI logic immediately so it stops moving/shooting
        if (ai != null) ai.enabled = false; 
        
        // Inform GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyKilled();
        }

        Destroy(gameObject); 
    }
}