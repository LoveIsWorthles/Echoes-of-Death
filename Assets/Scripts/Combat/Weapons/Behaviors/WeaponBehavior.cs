using UnityEngine;

public abstract class WeaponBehavior : ScriptableObject
{
    public abstract void Fire(WeaponDefinition weapon, WeaponFireContext context);

    protected static bool ValidateFire(WeaponDefinition weapon, WeaponBehavior caller)
    {
        if (weapon == null)
        {
            Debug.LogWarning($"{caller.GetType().Name}.Fire called with null weapon.", caller);
            return false;
        }

        if (weapon.projectilePrefab == null)
        {
            string label = string.IsNullOrEmpty(weapon.displayName) ? weapon.name : weapon.displayName;
            Debug.LogWarning($"Weapon '{label}' has no projectilePrefab assigned.", caller);
            return false;
        }

        return true;
    }

    protected static Projectile SpawnProjectile(WeaponDefinition weapon, WeaponFireContext context, Vector3 direction)
    {
        Quaternion rotation = Quaternion.LookRotation(direction);
        GameObject instance = Object.Instantiate(weapon.projectilePrefab, context.FirePoint, rotation, context.ProjectileParent);
        if (instance == null)
        {
            Debug.LogWarning("Failed to instantiate projectile prefab.", weapon);
            return null;
        }

        Projectile projectile = instance.GetComponent<Projectile>() ?? instance.GetComponentInChildren<Projectile>();
        if (projectile == null)
        {
            Debug.LogWarning("Projectile prefab does not contain a Projectile component.", instance);
            Object.Destroy(instance);
            return null;
        }

        projectile.Initialize(
            context.Owner,
            weapon,
            context.SourceFaction,
            weapon.damage,
            weapon.damageType,
            direction,
            weapon.projectileSpeed,
            weapon.projectileLifetime,
            weapon.maxRange,
            weapon.piercingMode,
            weapon.pierceCount,
            weapon.friendlyFire);

        return projectile;
    }

    protected static void PlayFireFx(WeaponDefinition weapon, WeaponFireContext context)
    {
        if (weapon.muzzleFlashPrefab != null)
        {
            GameObject fx = Object.Instantiate(
                weapon.muzzleFlashPrefab,
                context.FirePoint,
                context.FireRotation,
                context.ProjectileParent);

            if (fx != null)
            {
                Object.Destroy(fx, 3f);
            }
        }

        if (weapon.fireSound != null)
        {
            AudioSource.PlayClipAtPoint(weapon.fireSound, context.FirePoint);
        }
    }
}
