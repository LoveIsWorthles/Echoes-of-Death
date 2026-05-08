using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapons/Behaviors/Projectile", fileName = "SingleProjectileBehavior")]
public class SingleProjectileBehavior : WeaponBehavior
{
    public override void Fire(WeaponDefinition weapon, WeaponFireContext context)
    {
        Debug.LogWarning("SingleProjectileBehavior.Fire is not implemented yet.", this);
    }
}
