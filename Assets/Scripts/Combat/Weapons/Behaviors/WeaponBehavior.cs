using UnityEngine;

public abstract class WeaponBehavior : ScriptableObject
{
    public abstract void Fire(WeaponDefinition weapon, WeaponFireContext context);
}