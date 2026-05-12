using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrownGrenade : MonoBehaviour
{
    [Header("AOE")]
    [SerializeField] private LayerMask affectMask = ~0;
    [SerializeField, Min(0f)] private float cylinderDown = 1f;
    [SerializeField, Min(0f)] private float cylinderUp = 5f;

    [Header("Radius Indicator")]
    [SerializeField, Min(3)] private int ringSegments = 32;
    [SerializeField] private float ringWidth = 0.05f;
    [SerializeField] private Color ringColorSafe = Color.green;
    [SerializeField] private Color ringColorDanger = Color.red;
    [SerializeField, Range(0f, 1f)] private float dangerThreshold = 0.4f;

    [Header("Optional VFX")]
    [SerializeField] private GameObject fragVfxPrefab;
    [SerializeField] private GameObject flashVfxPrefab;

    [Header("Debug")]
    [SerializeField] private bool debugLogging;

    private Rigidbody rb;
    private LineRenderer ring;
    private GrenadeDefinition definition;
    private Faction ownerFaction;
    private GameObject owner;
    private bool isInitialized;
    private bool hasDetonated;
    private float fuseStartTime;
    private float fuseDuration;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        ring = gameObject.AddComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.startWidth = ringWidth;
        ring.endWidth = ringWidth;
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.receiveShadows = false;
        ring.material = new Material(Shader.Find("Sprites/Default"));
        ring.enabled = false;
    }

    public void Initialize(GrenadeDefinition def, Vector3 targetPos, Faction faction, GameObject thrower)
    {
        definition = def;
        ownerFaction = faction;
        owner = thrower;
        isInitialized = true;
        fuseStartTime = Time.time;
        fuseDuration = def.fuseTime;

        if (owner != null)
        {
            foreach (Collider ownerCol in owner.GetComponentsInChildren<Collider>())
            {
                foreach (Collider selfCol in GetComponentsInChildren<Collider>())
                {
                    Physics.IgnoreCollision(selfCol, ownerCol, true);
                }
            }
        }

        rb.linearVelocity = ComputeLaunchVelocity(transform.position, targetPos, def.throwSpeed);

        BuildRing(def.radius);
        ring.enabled = true;

        StartCoroutine(FuseRoutine(def.fuseTime));
    }

    private void Update()
    {
        if (!isInitialized || hasDetonated || ring == null) return;

        float elapsed = Time.time - fuseStartTime;
        float t = fuseDuration > 0f ? Mathf.Clamp01(elapsed / fuseDuration) : 1f;

        Color ringColor;
        if (t >= 1f - dangerThreshold)
        {
            float blink = Mathf.Sin(Time.time * 20f) > 0f ? 1f : 0f;
            ringColor = Color.Lerp(ringColorDanger, Color.white, blink * 0.3f);
        }
        else
        {
            ringColor = Color.Lerp(ringColorSafe, ringColorDanger, t / (1f - dangerThreshold));
        }

        ring.startColor = ringColor;
        ring.endColor = ringColor;
    }

    private void BuildRing(float radius)
    {
        ring.positionCount = ringSegments;
        float angleStep = 360f / ringSegments;
        for (int i = 0; i < ringSegments; i++)
        {
            float rad = Mathf.Deg2Rad * i * angleStep;
            ring.SetPosition(i, new Vector3(Mathf.Sin(rad) * radius, 0.02f, Mathf.Cos(rad) * radius));
        }
    }

    private static Vector3 ComputeLaunchVelocity(Vector3 from, Vector3 to, float speed)
    {
        Vector3 delta = to - from;
        Vector3 horizDelta = new Vector3(delta.x, 0f, delta.z);
        float d = horizDelta.magnitude;
        float h = delta.y;
        float g = Mathf.Abs(Physics.gravity.y);

        if (d < 0.01f || speed <= 0f || g <= 0f)
        {
            return Vector3.up * speed;
        }

        Vector3 horizDir = horizDelta / d;
        float v2 = speed * speed;
        float discriminant = v2 * v2 - g * (g * d * d + 2f * h * v2);

        if (discriminant < 0f)
        {
            return (to - from).normalized * speed;
        }

        float root = Mathf.Sqrt(discriminant);
        float tanLow = (v2 - root) / (g * d);
        float angle = Mathf.Atan(tanLow);
        float vx = speed * Mathf.Cos(angle);
        float vy = speed * Mathf.Sin(angle);

        return horizDir * vx + Vector3.up * vy;
    }

    private void DrawExplosionDebug(Vector3 center, float radius, float duration)
    {
        Vector3 low  = center + Vector3.down * cylinderDown;
        Vector3 high = center + Vector3.up   * cylinderUp;
        Color col = new Color(1f, 0.3f, 0f);

        int segments = 24;
        float step = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float a0 = Mathf.Deg2Rad * i * step;
            float a1 = Mathf.Deg2Rad * (i + 1) * step;
            Vector3 p0 = new Vector3(Mathf.Sin(a0) * radius, 0f, Mathf.Cos(a0) * radius);
            Vector3 p1 = new Vector3(Mathf.Sin(a1) * radius, 0f, Mathf.Cos(a1) * radius);
            Debug.DrawLine(low  + p0, low  + p1, col, duration);
            Debug.DrawLine(high + p0, high + p1, col, duration);
        }

        Vector3[] cardinals = { Vector3.forward * radius, Vector3.back * radius, Vector3.left * radius, Vector3.right * radius };
        foreach (Vector3 o in cardinals)
        {
            Debug.DrawLine(low + o, high + o, col, duration);
        }
    }

    private IEnumerator FuseRoutine(float fuseTime)
    {
        yield return new WaitForSeconds(fuseTime);
        Detonate();
    }

    private void OnDrawGizmos()
    {
        if (!isInitialized || hasDetonated || definition == null) return;

        float r = definition.radius;
        Vector3 pos = transform.position;
        Vector3 low  = pos + Vector3.down * cylinderDown;
        Vector3 high = pos + Vector3.up   * cylinderUp;

        Gizmos.color = new Color(1f, 0.3f, 0f, 0.35f);
        Gizmos.DrawSphere(low,  r);
        Gizmos.DrawSphere(high, r);

        Gizmos.color = new Color(1f, 0.3f, 0f, 0.6f);
        Gizmos.DrawWireSphere(low,  r);
        Gizmos.DrawWireSphere(high, r);

        Vector3[] offsets = { Vector3.forward * r, Vector3.back * r, Vector3.left * r, Vector3.right * r };
        foreach (Vector3 o in offsets)
        {
            Gizmos.DrawLine(low + o, high + o);
        }
    }

    private void Detonate()
    {
        if (hasDetonated || !isInitialized) return;
        hasDetonated = true;

        if (ring != null) ring.enabled = false;

        Vector3 center = transform.position;
        Vector3 low = center + Vector3.down * cylinderDown;
        Vector3 high = center + Vector3.up * cylinderUp;

        Collider[] hits = Physics.OverlapCapsule(low, high, definition.radius, affectMask, QueryTriggerInteraction.Ignore);

        if (debugLogging)
        {
            Debug.Log($"[ThrownGrenade] {definition.displayName} detonated. radius={definition.radius} hits={hits.Length}");
            DrawExplosionDebug(center, definition.radius, 2f);
        }

        HashSet<int> processedRoots = new HashSet<int>();
        bool isFrag = definition.grenadeType == GrenadeType.Frag;

        foreach (Collider col in hits)
        {
            if (col == null) continue;

            int rootId = col.transform.root.GetInstanceID();
            if (!processedRoots.Add(rootId)) continue;

            if (isFrag)
            {
                IDamageable damageable = col.GetComponentInParent<IDamageable>();
                if (damageable == null) continue;

                Vector3 hitPoint = col.ClosestPoint(center);
                Vector3 incoming = (col.bounds.center - center).sqrMagnitude > 0.0001f
                    ? (col.bounds.center - center).normalized
                    : Vector3.up;

                DamageInfo info = new DamageInfo(
                    definition.damage,
                    DamageType.Explosive,
                    owner,
                    definition,
                    hitPoint,
                    incoming,
                    ownerFaction);

                damageable.TakeDamage(info);
            }
            else
            {
                EnemyGuardAI ai = col.GetComponentInParent<EnemyGuardAI>();
                if (ai == null) continue;

                ai.Stun(definition.StunDuration);
            }
        }

        GameObject vfxPrefab = isFrag ? fragVfxPrefab : flashVfxPrefab;
        if (vfxPrefab != null)
        {
            Instantiate(vfxPrefab, center, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
