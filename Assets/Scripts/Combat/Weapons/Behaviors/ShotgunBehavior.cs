using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapons/Behaviors/Shotgun", fileName = "ShotgunBehavior")]
public class ShotgunBehavior : WeaponBehavior
{
    public override void Fire(WeaponDefinition weapon, WeaponFireContext context)
    {
        if (weapon == null)
        {
            Debug.LogWarning("ShotgunBehavior.Fire called with null weapon.", this);
            return;
        }

        if (weapon.projectilePrefab == null)
        {
            Debug.LogWarning($"Weapon '{(string.IsNullOrEmpty(weapon.displayName) ? weapon.name : weapon.displayName)}' has no projectilePrefab assigned.", this);
            return;
        }

        Vector3 baseDirection = context.GetDirection();
        int count = Mathf.Max(1, weapon.projectileCount);
        float spread = Mathf.Max(0f, weapon.spreadAngle);
        Vector3 spawnPos = context.FirePoint;
        Transform parent = context.ProjectileParent;

        for (int i = 0; i < count; i++)
        {
            float yaw = 0f;
            float pitch = 0f;

            if (count == 1)
            {
                float half = spread * 0.5f;
                yaw = Random.Range(-half, half);
                pitch = Random.Range(-half, half) * 0.25f;
            }
            else
            {
                float t = (float)i / (count - 1); // 0..1
                float centerYaw = Mathf.Lerp(-spread * 0.5f, spread * 0.5f, t);
                float jitter = Random.Range(-spread / (count * 3f), spread / (count * 3f));
                yaw = centerYaw + jitter;
                pitch = Random.Range(-spread * 0.5f, spread * 0.5f) * 0.15f;
            }

            Vector3 direction = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right) * baseDirection;
            Quaternion rot = Quaternion.LookRotation(direction);

            GameObject go = Object.Instantiate(weapon.projectilePrefab, spawnPos, rot, parent);
            if (go == null)
            {
                Debug.LogWarning("Failed to instantiate projectile prefab.", this);
                continue;
            }

            Projectile projectile = go.GetComponent<Projectile>() ?? go.GetComponentInChildren<Projectile>();
            if (projectile == null)
            {
                Debug.LogWarning("Projectile prefab does not contain a Projectile component.", go);
                Object.Destroy(go);
                continue;
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
        }

        if (weapon.muzzleFlashPrefab != null)
        {
            GameObject fx = Object.Instantiate(weapon.muzzleFlashPrefab, spawnPos, context.FireRotation, parent);
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
