using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Projectile : MonoBehaviour
{
    public enum PiercingMode
    {
        None,
        ActorsOnly,
        WallsOnly,
        ActorsAndWalls
    }

    [Header("Hit Filtering")]
    [SerializeField]
    private LayerMask actorLayers = ~0;

    [SerializeField]
    private LayerMask wallLayers = ~0;

    [Header("Debug")]
    [SerializeField]
    private bool debugLogging;

    private readonly HashSet<int> processedTargetIds = new HashSet<int>();

    private Rigidbody projectileRigidbody;
    private Collider projectileCollider;
    private GameObject owner;
    private ScriptableObject sourceDefinition;
    private Faction sourceFaction = Faction.Neutral;
    private int damageAmount = 1;
    private DamageType damageType = DamageType.Bullet;
    private Vector3 travelDirection = Vector3.forward;
    private float speed = 10f;
    private float lifetime;
    private float maxDistance;
    private PiercingMode piercingMode = PiercingMode.None;
    private int remainingPierceCount;
    private bool friendlyFireEnabled;
    private Vector3 spawnPosition;
    private float elapsedLifetime;
    private bool isInitialized;

    private void Awake()
    {
        projectileRigidbody = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<Collider>();

        projectileCollider.isTrigger = true;
        projectileRigidbody.useGravity = false;
        projectileRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        projectileRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        projectileRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void Initialize(
        GameObject ownerGameObject,
        ScriptableObject sourceDefinitionObject,
        Faction sourceFactionValue,
        int damageAmountValue,
        DamageType damageTypeValue,
        Vector3 direction,
        float speedValue,
        float lifetimeValue,
        float maxDistanceValue,
        PiercingMode piercingModeValue,
        int pierceCountValue,
        bool friendlyFireEnabledValue)
    {
        owner = ownerGameObject;
        sourceDefinition = sourceDefinitionObject;
        sourceFaction = sourceFactionValue;
        damageAmount = Mathf.Max(1, damageAmountValue);
        damageType = damageTypeValue;
        travelDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
        speed = Mathf.Max(0f, speedValue);
        lifetime = Mathf.Max(0f, lifetimeValue);
        maxDistance = Mathf.Max(0f, maxDistanceValue);
        piercingMode = piercingModeValue;
        remainingPierceCount = Mathf.Max(0, pierceCountValue);
        friendlyFireEnabled = friendlyFireEnabledValue;
        elapsedLifetime = 0f;
        spawnPosition = projectileRigidbody.position;
        processedTargetIds.Clear();
        isInitialized = true;

        if (travelDirection.sqrMagnitude > 0f)
        {
            transform.forward = travelDirection;
        }

        IgnoreOwnerColliders();
        projectileRigidbody.linearVelocity = travelDirection * speed;
    }

    private void FixedUpdate()
    {
        if (!isInitialized)
        {
            return;
        }

        elapsedLifetime += Time.fixedDeltaTime;

        if (lifetime > 0f && elapsedLifetime >= lifetime)
        {
            Log("Projectile destroyed because its lifetime expired.");
            Destroy(gameObject);
            return;
        }

        if (maxDistance > 0f)
        {
            float traveledDistance = Vector3.Distance(spawnPosition, projectileRigidbody.position);
            if (traveledDistance >= maxDistance)
            {
                Log("Projectile destroyed because it reached max distance.");
                Destroy(gameObject);
                return;
            }
        }

        projectileRigidbody.linearVelocity = travelDirection * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isInitialized || other == null)
        {
            return;
        }

        if (other == projectileCollider || IsOwnerCollider(other))
        {
            return;
        }

        GameObject contactObject = GetContactObject(other);
        if (contactObject != null && processedTargetIds.Contains(contactObject.GetInstanceID()))
        {
            return;
        }

        if (TryGetDamageable(other, out IDamageable damageable, out GameObject damageableObject))
        {
            if (!IsInLayerMask(other.gameObject.layer, actorLayers))
            {
                return;
            }

            if (!friendlyFireEnabled && IsSameFaction(damageableObject))
            {
                MarkTargetProcessed(damageableObject);
                Log("Projectile ignored a same-faction target.");
                return;
            }

            ApplyDamage(damageable, other);
            MarkTargetProcessed(damageableObject);

            if (!CanPierceActor())
            {
                Log("Projectile destroyed after hitting an actor.");
                Destroy(gameObject);
            }

            return;
        }

        if (!IsInLayerMask(other.gameObject.layer, wallLayers))
        {
            return;
        }

        MarkTargetProcessed(contactObject ?? other.gameObject);

        if (!CanPierceWall())
        {
            Log("Projectile destroyed after hitting a wall.");
            Destroy(gameObject);
        }
    }

    private void ApplyDamage(IDamageable damageable, Collider hitCollider)
    {
        Vector3 hitPoint = hitCollider.ClosestPoint(projectileRigidbody.position);

        damageable.TakeDamage(
            new DamageInfo(
                damageAmount,
                damageType,
                owner,
                sourceDefinition,
                hitPoint,
                travelDirection,
                sourceFaction));

        Log($"Projectile dealt {damageAmount} damage to {hitCollider.name}.");
    }

    private bool CanPierceActor()
    {
        return CanConsumePierce(piercingMode == PiercingMode.ActorsOnly || piercingMode == PiercingMode.ActorsAndWalls);
    }

    private bool CanPierceWall()
    {
        return CanConsumePierce(piercingMode == PiercingMode.WallsOnly || piercingMode == PiercingMode.ActorsAndWalls);
    }

    private bool CanConsumePierce(bool piercingTypeAllowed)
    {
        if (piercingMode == PiercingMode.None || !piercingTypeAllowed)
        {
            return false;
        }

        if (remainingPierceCount <= 0)
        {
            return false;
        }

        remainingPierceCount--;
        Log($"Projectile consumed a pierce. Remaining pierces: {remainingPierceCount}.");
        return true;
    }

    private bool IsSameFaction(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return false;
        }

        if (!TryGetFaction(targetObject, out Faction targetFaction))
        {
            targetFaction = Faction.Neutral;
        }

        return targetFaction == sourceFaction;
    }

    private bool TryGetDamageable(Collider other, out IDamageable damageable, out GameObject damageableObject)
    {
        MonoBehaviour[] behaviours = other.GetComponentsInParent<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IDamageable foundDamageable)
            {
                damageable = foundDamageable;
                damageableObject = behaviour.gameObject;
                return true;
            }
        }

        damageable = null;
        damageableObject = null;
        return false;
    }

    private bool TryGetFaction(GameObject targetObject, out Faction faction)
    {
        FactionMember factionMember = targetObject.GetComponentInParent<FactionMember>();

        if (factionMember != null)
        {
            faction = factionMember.Faction;
            return true;
        }

        faction = Faction.Neutral;
        return false;
    }

    private bool IsOwnerCollider(Collider other)
    {
        if (owner == null)
        {
            return false;
        }

        return other.transform.IsChildOf(owner.transform) || other.transform.root.gameObject == owner;
    }

    private GameObject GetContactObject(Collider other)
    {
        return other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.transform.root.gameObject;
    }

    public void IgnoreCollisionWith(Projectile other)
    {
        if (other != null && other.projectileCollider != null)
            Physics.IgnoreCollision(projectileCollider, other.projectileCollider, true);
    }

    private void IgnoreOwnerColliders()
    {
        if (owner == null)
        {
            return;
        }

        Collider[] ownerColliders = owner.GetComponentsInChildren<Collider>(true);
        foreach (Collider ownerCollider in ownerColliders)
        {
            if (ownerCollider != null)
            {
                Physics.IgnoreCollision(projectileCollider, ownerCollider, true);
            }
        }
    }

    private void MarkTargetProcessed(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return;
        }

        processedTargetIds.Add(targetObject.GetInstanceID());
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    private void Log(string message)
    {
        if (debugLogging)
        {
            Debug.Log(message, this);
        }
    }
}
