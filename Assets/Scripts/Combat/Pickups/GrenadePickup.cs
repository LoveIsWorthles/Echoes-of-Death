using UnityEngine;

public class GrenadePickup : Pickup
{
    [SerializeField]
    private GrenadeDefinition grenade;

    [SerializeField, Min(1)]
    private int amount = 1;

    protected override bool TryPickup(Collider other)
    {
        if (grenade == null)
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
}
