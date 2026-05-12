using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Loadouts/Loadout Definition", fileName = "LoadoutDefinition")]
public class LoadoutDefinition : ScriptableObject
{
    [Header("Starting Primary (set one, not both)")]
    [SerializeField]
    private WeaponDefinition startingPrimary;

    [SerializeField]
    private ShieldDefinition startingShield;

    [Header("Starting Secondary")]
    [SerializeField]
    private WeaponDefinition startingSecondary;

    [SerializeField]
    private bool startWithSecondaryActive = true;

    [Header("Notes")]
    [SerializeField, TextArea]
    private string notes;

    public WeaponDefinition StartingPrimary => startingPrimary;

    public ShieldDefinition StartingShield => startingShield;

    public WeaponDefinition StartingSecondary => startingSecondary;

    public bool StartWithSecondaryActive => startWithSecondaryActive;
}
