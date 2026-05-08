using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapons/Behaviors/Projectile", fileName = "SingleProjectileBehavior")]
public class SingleProjectileBehavior : WeaponBehavior
{
    public override void Fire(WeaponDefinition weapon, WeaponFireContext context)
    {
        if (weapon == null)
        {
            Debug.LogWarning("SingleProjectileBehavior.Fire called with null weapon.", this);
            return;
        }

        if (weapon.projectilePrefab == null)
        {
            Debug.LogWarning($"Weapon '{(string.IsNullOrEmpty(weapon.displayName) ? weapon.name : weapon.displayName)}' has no projectilePrefab assigned.", this);
            return;
        }

        Vector3 baseDirection = context.GetDirection();
        float spread = Mathf.Max(0f, weapon.spreadAngle);

        Vector3 direction = baseDirection;
        if (spread > 0f)
        {
            float half = spread * 0.5f;
            float yaw = Random.Range(-half, half);
            float pitch = Random.Range(-half, half);
            direction = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right) * baseDirection;
        }

        Quaternion rot = Quaternion.LookRotation(direction);
        Vector3 spawnPos = context.FirePoint;
        Transform parent = context.ProjectileParent;

        GameObject go = Object.Instantiate(weapon.projectilePrefab, spawnPos, rot, parent);
        if (go == null)
        {
            Debug.LogWarning("Failed to instantiate projectile prefab.", this);
            return;
        }

        Projectile projectile = go.GetComponent<Projectile>() ?? go.GetComponentInChildren<Projectile>();
        if (projectile == null)
        {
            Debug.LogWarning("Projectile prefab does not contain a Projectile component.", go);
            return;
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

        if (weapon.muzzleFlashPrefab != null)
        {
            GameObject fx = Object.Instantiate(weapon.muzzleFlashPrefab, spawnPos, rot, parent);
            if (fx != null)
            {
                Object.Destroy(fx, 3f);
            }
        }

        if (weapon.fireSound != null)
        {
            AudioSource.PlayClipAtPoint(weapon.fireSound, spawnPos);
        }
    }
}
