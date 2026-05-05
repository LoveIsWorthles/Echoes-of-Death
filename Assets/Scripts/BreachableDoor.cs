using UnityEngine;

public class BreachableDoor : MonoBehaviour, IDamagable
{
    [Header("Door Stats")]
    public float health = 100f; // Doors might be tougher than a single wall brick

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        
        if (health <= 0f)
        {
            // Note: Later we can spawn a "wood splinters" particle effect here!
            Destroy(gameObject);
        }
    }
}