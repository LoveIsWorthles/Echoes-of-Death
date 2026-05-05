using UnityEngine;

public class WallChunk : MonoBehaviour, IDamagable
{
    [Header("Chunk Stats")]
    public float health = 50f;

    // This function will be called by the grenade
    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        
        if (health <= 0f)
        {
            // Optional: You could instantiate a tiny particle dust cloud here later!
            Destroy(gameObject);
        }
    }
}