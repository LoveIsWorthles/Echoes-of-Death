using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Explosive Settings")]
    public float fuseTime = 3f;
    public float blastRadius = 2.5f;
    public float damage = 100f;

    void Start()
    {
        // Start the countdown timer the moment the grenade spawns
        Invoke(nameof(Explode), fuseTime);
    }

    void Explode()
    {
        // 1. Create an invisible sphere and find EVERYTHING it touches
        Collider[] objectsInBlast = Physics.OverlapSphere(transform.position, blastRadius);

        // 2. Loop through every object we hit
        foreach (Collider hitObject in objectsInBlast)
        {
            // 3. Check if the object is a universal IDamagable contract
            IDamagable target = hitObject.GetComponent<IDamagable>();            
            if (target != null)
            {
                // 4. Deal damage to it!
                target.TakeDamage(damage); // Hurt the wall, door, or enemy!
            }
            
        }

        // Destroy the grenade object itself
        Destroy(gameObject);
    }

    // This draws a red wireframe circle in the Unity Editor so you can physically see the blast radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}