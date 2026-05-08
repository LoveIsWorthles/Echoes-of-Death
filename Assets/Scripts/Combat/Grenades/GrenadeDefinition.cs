using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Grenades/Grenade Definition", fileName = "GrenadeDefinition")]
public class GrenadeDefinition : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public string grenadeId;
    public GrenadeType grenadeType = GrenadeType.Frag;
    public Sprite icon;

    [Header("Future Projectile")]
    public GameObject grenadeProjectilePrefab;

    [Header("Throw Settings")]
    [Min(0f)] public float throwSpeed = 12f;
    [Min(0f)] public float throwRange = 12f;
    [Min(0f)] public float fuseTime = 3f;
    [Min(0f)] public float radius = 2.5f;
    [Min(0)] public int damage = 0;
    [Min(0)] public int maxCarryCount = 3;
}
