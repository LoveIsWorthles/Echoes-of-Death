using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Shields/Shield Definition", fileName = "ShieldDefinition")]
public class ShieldDefinition : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public Sprite icon;

    [Header("Charges")]
    [Min(1)] public int maxCharges = 5;

    [Header("Block Angles (degrees, full cone width)")]
    [Range(0f, 360f)] public float frontBlockAngle = 120f;
    [Range(0f, 360f)] public float backBlockAngle = 60f;
}
