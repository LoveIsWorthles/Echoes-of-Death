using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GrenadePickup : MonoBehaviour
{
    [SerializeField]
    private GrenadeDefinition grenade;

    [SerializeField]
    [Min(1)]
    private int amount = 1;

    private void Reset()
    {
        ConfigureTrigger();
    }

    private void OnValidate()
    {
        ConfigureTrigger();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TryPickup(other))
        {
            Destroy(gameObject);
        }
    }

    private bool TryPickup(Collider other)
    {
        if (grenade == null || amount <= 0)
        {
            return false;
        }

        GrenadeSlotController grenadeSlots = other.GetComponentInParent<GrenadeSlotController>();
        if (grenadeSlots == null)
        {
            return false;
        }

        grenadeSlots.AddGrenades(grenade, amount);
        return true;
    }

    private void ConfigureTrigger()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }
}
