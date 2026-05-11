using UnityEngine;

public class WallChunk : MonoBehaviour, IDamageable
{
    [Header("Chunk Stats")]
    public int health = 50;

    // This function will be called by the grenade
    public void TakeDamage(DamageInfo damageInfo)
    {
        health -= damageInfo.Amount;
        
        if (health <= 0)
        {
            // Optional: You could instantiate a tiny particle dust cloud here later!
            Destroy(gameObject);
        }
    }
}