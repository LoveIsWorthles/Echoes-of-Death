using UnityEngine;

[System.Serializable]
public struct WeaponFireContext
{
    public GameObject Owner;
    public Object SourceDefinition;
    public Faction SourceFaction;
    public Vector3 FirePoint;
    public Quaternion FireRotation;
    public Vector3 Direction;
    public Transform ProjectileParent;

    public WeaponFireContext(
        GameObject owner,
        Object sourceDefinition,
        Faction sourceFaction,
        Vector3 firePoint,
        Quaternion fireRotation,
        Vector3 direction,
        Transform projectileParent)
    {
        Owner = owner;
        SourceDefinition = sourceDefinition;
        SourceFaction = sourceFaction;
        FirePoint = firePoint;
        FireRotation = fireRotation;
        Direction = direction;
        ProjectileParent = projectileParent;
    }

    public Vector3 GetDirection()
    {
        if (Direction.sqrMagnitude > 0.0001f)
        {
            return Direction.normalized;
        }

        return (FireRotation * Vector3.forward).normalized;
    }
}
