using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapons/Behaviors/Shotgun", fileName = "ShotgunBehavior")]
public class ShotgunBehavior : WeaponBehavior
{
    public override void Fire(WeaponDefinition weapon, WeaponFireContext context)
    {
        if (!ValidateFire(weapon, this))
        {
            return;
        }

        Vector3 baseDirection = context.GetDirection();
        int count = Mathf.Max(1, weapon.projectileCount);
        float spread = Mathf.Max(0f, weapon.spreadAngle);

        Projectile[] pellets = new Projectile[count];
        for (int i = 0; i < count; i++)
        {
            Vector3 direction = ComputeShotDirection(baseDirection, count, i, spread);
            pellets[i] = SpawnProjectile(weapon, context, direction);
        }

        for (int i = 0; i < pellets.Length; i++)
        {
            if (pellets[i] == null) continue;
            for (int j = i + 1; j < pellets.Length; j++)
            {
                if (pellets[j] != null)
                    pellets[i].IgnoreCollisionWith(pellets[j]);
            }
        }

        PlayFireFx(weapon, context);
    }

    private static Vector3 ComputeShotDirection(Vector3 baseDirection, int count, int index, float spread)
    {
        float yaw;
        float pitch;

        if (count == 1)
        {
            float half = spread * 0.5f;
            yaw = Random.Range(-half, half);
            pitch = Random.Range(-half, half) * 0.25f;
        }
        else
        {
            float t = (float)index / (count - 1);
            float centerYaw = Mathf.Lerp(-spread * 0.5f, spread * 0.5f, t);
            float jitter = Random.Range(-spread / (count * 3f), spread / (count * 3f));
            yaw = centerYaw + jitter;
            pitch = Random.Range(-spread * 0.5f, spread * 0.5f) * 0.15f;
        }

        return Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right) * baseDirection;
    }
}
