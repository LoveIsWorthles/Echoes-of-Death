using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapons/Behaviors/Shotgun", fileName = "ShotgunBehavior")]
public class ShotgunBehavior : WeaponBehavior
{
    public override void Fire(WeaponDefinition weapon, WeaponFireContext context)
    {
        Debug.LogWarning("ShotgunBehavior.Fire is not implemented yet.", this);
    }
}
