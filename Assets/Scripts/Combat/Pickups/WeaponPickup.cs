using UnityEngine;

public class WeaponPickup : Pickup
{
    [SerializeField]
    private WeaponDefinition weapon;

    protected override bool TryPickup(Collider other)
    {
        if (weapon == null || weapon.slot != WeaponSlot.Primary)
        {
            return false;
        }

        LoadoutController loadout = other.GetComponentInParent<LoadoutController>();
        if (loadout == null)
        {
            return false;
        }

        ShieldBlocker shieldBlocker = null;
        if (weapon.IsShield)
        {
            shieldBlocker = other.GetComponentInParent<ShieldBlocker>();
            if (shieldBlocker == null)
            {
                return false;
            }
        }

        if (!loadout.EquipPrimary(weapon, makeActive: true))
        {
            return false;
        }

        if (shieldBlocker != null)
        {
            shieldBlocker.ReplaceWithFreshShield(weapon);
        }

        return true;
    }
}
