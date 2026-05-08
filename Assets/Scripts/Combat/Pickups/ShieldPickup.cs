using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShieldPickup : MonoBehaviour
{
    [SerializeField]
    private WeaponDefinition shieldWeapon;

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
        if (!IsValidShield(shieldWeapon))
        {
            return false;
        }

        LoadoutController loadout = other.GetComponentInParent<LoadoutController>();
        ShieldBlocker shieldBlocker = other.GetComponentInParent<ShieldBlocker>();

        if (loadout == null || shieldBlocker == null)
        {
            return false;
        }

        if (!loadout.EquipPrimary(shieldWeapon, makeActive: true))
        {
            return false;
        }

        shieldBlocker.ReplaceWithFreshShield(shieldWeapon);
        return true;
    }

    private static bool IsValidShield(WeaponDefinition definition)
    {
        return definition != null
            && definition.slot == WeaponSlot.Primary
            && (definition.kind == WeaponKind.Defensive || definition.isDefensiveOnly);
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
