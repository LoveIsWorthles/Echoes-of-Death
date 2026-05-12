using UnityEngine;

public class GrenadeThrower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GrenadeSlotController slotController;
    [SerializeField] private FactionMember faction;
    [SerializeField] private Transform throwOrigin;

    [Header("Debug")]
    [SerializeField] private bool debugLogging;

    private void Reset()
    {
        slotController = GetComponent<GrenadeSlotController>();
        faction = GetComponent<FactionMember>();
        throwOrigin = transform;
    }

    private void Awake()
    {
        if (slotController == null) slotController = GetComponent<GrenadeSlotController>();
        if (faction == null) faction = GetComponent<FactionMember>();
        if (throwOrigin == null) throwOrigin = transform;
    }

    private void OnEnable()
    {
        if (slotController != null)
        {
            slotController.GrenadeThrown += HandleGrenadeThrown;
        }
    }

    private void OnDisable()
    {
        if (slotController != null)
        {
            slotController.GrenadeThrown -= HandleGrenadeThrown;
        }
    }

    private void HandleGrenadeThrown(GrenadeDefinition definition, GrenadeType type, Vector3 targetWorldPosition)
    {
        if (definition == null || definition.grenadeProjectilePrefab == null)
        {
            Debug.LogWarning($"[GrenadeThrower] Missing definition or projectile prefab for grenade type {type}.");
            return;
        }

        Vector3 origin = throwOrigin.position;
        Vector3 clampedTarget = ClampToThrowRange(origin, targetWorldPosition, definition.throwRange);

        GameObject instance = Instantiate(definition.grenadeProjectilePrefab, origin, Quaternion.identity);
        ThrownGrenade thrown = instance.GetComponent<ThrownGrenade>();

        if (thrown == null)
        {
            Debug.LogError($"[GrenadeThrower] Prefab '{definition.grenadeProjectilePrefab.name}' is missing a ThrownGrenade component.");
            Destroy(instance);
            return;
        }

        Faction throwerFaction = faction != null ? faction.Faction : Faction.Neutral;
        thrown.Initialize(definition, clampedTarget, throwerFaction, gameObject);

        if (debugLogging)
        {
            Debug.Log($"[GrenadeThrower] Threw {definition.displayName} toward {clampedTarget}");
        }
    }

    private static Vector3 ClampToThrowRange(Vector3 origin, Vector3 target, float maxRange)
    {
        if (maxRange <= 0f) return target;

        Vector3 horizDelta = new Vector3(target.x - origin.x, 0f, target.z - origin.z);
        float dist = horizDelta.magnitude;
        if (dist <= maxRange) return target;

        Vector3 clampedHoriz = horizDelta * (maxRange / dist);
        return new Vector3(origin.x + clampedHoriz.x, target.y, origin.z + clampedHoriz.z);
    }
}
