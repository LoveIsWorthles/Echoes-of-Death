using UnityEngine;

public class BreachableDoor : MonoBehaviour, IDamageable
{
    [Header("Door Stats")]
    public int health = 100; // Doors might be tougher than a single wall brick

    public void TakeDamage(DamageInfo damageInfo)
    {
        health -= damageInfo.Amount;
        
        if (health <= 0)
        {
            // Note: Later we can spawn a "wood splinters" particle effect here!
            Destroy(gameObject);
        }
    }
}