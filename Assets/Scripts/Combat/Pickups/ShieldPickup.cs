using UnityEngine;

public class ShieldPickup : Pickup
{
    [SerializeField]
    private ShieldDefinition shield;

    protected override bool TryPickup(Collider other)
    {
        if (shield == null)
        {
            return false;
        }

        LoadoutController loadout = other.GetComponentInParent<LoadoutController>();
        if (loadout == null)
        {
            return false;
        }

        if (!loadout.HasPrimaryShield)
        {
            return loadout.EquipShield(shield);
        }

        return loadout.RefillShield();
    }
}
