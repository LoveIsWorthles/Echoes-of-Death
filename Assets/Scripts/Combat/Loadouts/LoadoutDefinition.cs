using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Loadouts/Loadout Definition", fileName = "LoadoutDefinition")]
public class LoadoutDefinition : ScriptableObject
{
    [Header("Starting Weapons")]
    [SerializeField]
    private WeaponDefinition startingPrimary;
    
    [SerializeField]
    private WeaponDefinition startingSecondary;
    
    [SerializeField]
    private bool startWithSecondaryActive = true;

    [Header("Notes")]
    [SerializeField, TextArea]
    private string notes;

    public WeaponDefinition StartingPrimary => startingPrimary;

    public WeaponDefinition StartingSecondary => startingSecondary;

    public bool StartWithSecondaryActive => startWithSecondaryActive;
}
