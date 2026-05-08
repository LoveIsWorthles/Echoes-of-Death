using UnityEngine;

[System.Serializable]
public struct DamageInfo
{
    public int Amount;
    public DamageType DamageType;
    public GameObject SourceOwner;
    public ScriptableObject SourceDefinition;
    public Vector3 HitPoint;
    public Vector3 IncomingDirection;
    public Faction SourceFaction;

    public DamageInfo(
        int amount,
        DamageType damageType,
        GameObject sourceOwner,
        ScriptableObject sourceDefinition,
        Vector3 hitPoint,
        Vector3 incomingDirection,
        Faction sourceFaction)
    {
        Amount = amount;
        DamageType = damageType;
        SourceOwner = sourceOwner;
        SourceDefinition = sourceDefinition;
        HitPoint = hitPoint;
        IncomingDirection = incomingDirection;
        SourceFaction = sourceFaction;
    }

    public static DamageInfo FromAmount(int amount)
    {
        return new DamageInfo(amount, DamageType.Bullet, null, null, Vector3.zero, Vector3.zero, Faction.Neutral);
    }
}
