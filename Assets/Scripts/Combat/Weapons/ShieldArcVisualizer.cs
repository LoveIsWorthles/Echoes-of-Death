using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ShieldArcVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private LoadoutController loadout;

    [SerializeField]
    private ShieldBlocker shieldBlocker;

    [Header("Geometry")]
    [SerializeField, Min(0.05f)]
    private float radius = 1.5f;

    [SerializeField]
    private float heightOffset = 0.1f;

    [SerializeField, Range(4, 128)]
    private int segments = 48;

    [Header("Colors")]
    [SerializeField]
    private Color frontArcColor = new Color(0.4f, 0.75f, 1f, 0.55f);

    [SerializeField]
    private Color rearArcColor = new Color(0.4f, 0.75f, 1f, 0.25f);

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh { name = "ShieldArc" };
        mesh.MarkDynamic();
        meshFilter.mesh = mesh;

        propertyBlock = new MaterialPropertyBlock();

        if (loadout == null)
        {
            loadout = GetComponentInParent<LoadoutController>();
        }
        if (shieldBlocker == null)
        {
            shieldBlocker = GetComponentInParent<ShieldBlocker>();
        }
    }

    private void OnEnable()
    {
        if (loadout != null)
        {
            loadout.ShieldEquipped += OnShieldEquipped;
            loadout.ShieldRemoved += OnShieldRemoved;
            loadout.ActiveSlotChanged += OnActiveSlotChanged;
        }

        if (shieldBlocker != null)
        {
            shieldBlocker.ShieldChargesChanged += OnShieldChargesChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (loadout != null)
        {
            loadout.ShieldEquipped -= OnShieldEquipped;
            loadout.ShieldRemoved -= OnShieldRemoved;
            loadout.ActiveSlotChanged -= OnActiveSlotChanged;
        }

        if (shieldBlocker != null)
        {
            shieldBlocker.ShieldChargesChanged -= OnShieldChargesChanged;
        }
    }

    private void OnShieldEquipped(ShieldDefinition _) => Refresh();
    private void OnShieldRemoved() => Refresh();
    private void OnActiveSlotChanged(WeaponSlot _) => Refresh();
    private void OnShieldChargesChanged(int current, int _) => Refresh();

    private void Refresh()
    {
        bool hasShield =
            loadout != null
            && loadout.PrimaryShield != null
            && shieldBlocker != null
            && shieldBlocker.CurrentCharges > 0;

        if (!hasShield)
        {
            meshRenderer.enabled = false;
            return;
        }

        bool shieldActive = loadout.ActiveSlot == WeaponSlot.Primary;
        ShieldDefinition def = loadout.PrimaryShield;
        float arcAngle = shieldActive ? def.frontBlockAngle : def.backBlockAngle;
        Color color = shieldActive ? frontArcColor : rearArcColor;
        Vector3 centerDirection = shieldActive ? Vector3.forward : Vector3.back;

        BuildMesh(centerDirection, arcAngle);

        transform.localPosition = new Vector3(0f, heightOffset, transform.localPosition.z);
        meshRenderer.enabled = true;

        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(BaseColorId, color);
        propertyBlock.SetColor(ColorId, color);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    private void BuildMesh(Vector3 centerDirection, float arcAngleDegrees)
    {
        mesh.Clear();

        int vertexCount = segments + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;

        float halfAngle = arcAngleDegrees * 0.5f;
        float baseAngle = Mathf.Atan2(centerDirection.x, centerDirection.z) * Mathf.Rad2Deg;
        float startAngle = baseAngle - halfAngle;
        float step = arcAngleDegrees / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angleDeg = startAngle + step * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Sin(angleRad) * radius, 0f, Mathf.Cos(angleRad) * radius);
        }

        for (int i = 0; i < segments; i++)
        {
            int triIndex = i * 3;
            triangles[triIndex] = 0;
            triangles[triIndex + 1] = i + 1;
            triangles[triIndex + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
