using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField]
    private WeaponDefinition currentWeapon;

    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private Transform projectileParent;

    [Header("Faction")]
    [SerializeField]
    private FactionMember factionMember;

    [SerializeField]
    private Faction fallbackFaction = Faction.Neutral;

    private float nextAllowedFireTime;

    public WeaponDefinition CurrentWeapon => currentWeapon;

    private void Awake()
    {
        if (factionMember == null)
        {
            factionMember = GetComponentInParent<FactionMember>();
        }
    }

    public void SetWeapon(WeaponDefinition weapon)
    {
        currentWeapon = weapon;
        nextAllowedFireTime = 0f;
    }

    public bool TryFire(Vector3 direction)
    {
        if (currentWeapon == null || currentWeapon.behavior == null)
        {
            return false;
        }

        if (currentWeapon.IsShield)
        {
            return false;
        }

        if (firePoint == null)
        {
            return false;
        }

        if (Time.time < nextAllowedFireTime)
        {
            return false;
        }

        Vector3 fireDirection = NormalizeFireDirection(direction);
        if (fireDirection.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        Quaternion fireRotation = Quaternion.LookRotation(fireDirection, Vector3.up);
        Faction sourceFaction = GetFaction();
        float cooldownSeconds = currentWeapon.fireRate > 0f ? 1f / currentWeapon.fireRate : 0f;

        WeaponFireContext context = new WeaponFireContext(
            gameObject,
            currentWeapon,
            sourceFaction,
            firePoint.position,
            fireRotation,
            fireDirection,
            projectileParent);

        currentWeapon.behavior.Fire(currentWeapon, context);
        nextAllowedFireTime = Time.time + cooldownSeconds;
        return true;
    }

    private Faction GetFaction()
    {
        if (factionMember != null)
        {
            return factionMember.Faction;
        }

        return fallbackFaction;
    }

    private Vector3 NormalizeFireDirection(Vector3 direction)
    {
        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);

        if (flatDirection.sqrMagnitude > 0.0001f)
        {
            return flatDirection.normalized;
        }

        return Vector3.zero;
    }
}
