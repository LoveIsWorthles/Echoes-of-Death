using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapons/Weapon Definition", fileName = "WeaponDefinition")]
public class WeaponDefinition : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public WeaponSlot slot = WeaponSlot.Primary;
    public WeaponKind kind = WeaponKind.Projectile;
    public Sprite icon;

    [Header("Behavior")]
    public WeaponBehavior behavior;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    [Min(1)] public int damage = 1;
    public DamageType damageType = DamageType.Bullet;
    [Min(0f)] public float fireRate = 2f;
    [Min(0f)] public float projectileSpeed = 20f;
    [Min(0f)] public float projectileLifetime = 2f;
    [Min(0f)] public float maxRange = 50f;
    [Min(1)] public int projectileCount = 1;
    [Min(0f)] public float spreadAngle;
    public Projectile.PiercingMode piercingMode = Projectile.PiercingMode.None;
    [Min(0)] public int pierceCount;
    public bool friendlyFire;

    [Header("Shield")]
    [Min(1)] public int shieldMaxCharges = 5;
    [Range(0f, 360f)] public float frontBlockAngle = 120f;
    [Range(0f, 360f)] public float backBlockAngle = 60f;

    [Header("FX")]
    public AudioClip fireSound;
    public GameObject muzzleFlashPrefab;

    public bool IsShield => kind == WeaponKind.Defensive;
}
