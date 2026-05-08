using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapons/Behaviors/Projectile", fileName = "SingleProjectileBehavior")]
public class SingleProjectileBehavior : WeaponBehavior
{
    public override void Fire(WeaponDefinition weapon, WeaponFireContext context)
    {
        if (!ValidateFire(weapon, this))
        {
            return;
        }

        Vector3 direction = ApplySpread(context.GetDirection(), weapon.spreadAngle);

        SpawnProjectile(weapon, context, direction);
        PlayFireFx(weapon, context);
    }

    private static Vector3 ApplySpread(Vector3 baseDirection, float spreadDegrees)
    {
        float spread = Mathf.Max(0f, spreadDegrees);
        if (spread <= 0f)
        {
            return baseDirection;
        }

        float half = spread * 0.5f;
        float yaw = Random.Range(-half, half);
        float pitch = Random.Range(-half, half);
        return Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right) * baseDirection;
    }
}
