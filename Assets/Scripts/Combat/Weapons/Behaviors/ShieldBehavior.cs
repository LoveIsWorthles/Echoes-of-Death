using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapons/Behaviors/Shield", fileName = "ShieldBehavior")]
public class ShieldBehavior : WeaponBehavior
{
    public override void Fire(WeaponDefinition weapon, WeaponFireContext context)
    {
        Debug.LogWarning("ShieldBehavior does not fire projectiles.", this);
    }
}
