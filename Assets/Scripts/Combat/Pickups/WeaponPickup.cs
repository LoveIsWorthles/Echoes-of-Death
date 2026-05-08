using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponPickup : MonoBehaviour
{
    [SerializeField]
    private WeaponDefinition weapon;

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
        if (IsShieldWeapon(weapon))
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

    private static bool IsShieldWeapon(WeaponDefinition definition)
    {
        return definition.kind == WeaponKind.Defensive || definition.isDefensiveOnly;
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
