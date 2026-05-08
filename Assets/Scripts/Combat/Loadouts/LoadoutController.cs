using UnityEngine;

using UnityEngine.Events;

public class LoadoutController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private WeaponController weaponController;

    [SerializeField]
    private LoadoutDefinition loadoutDefinition;

    [Header("State")]
    [SerializeField]
    private WeaponSlot activeSlot = WeaponSlot.Secondary;

    private WeaponDefinition primaryWeapon;
    private WeaponDefinition secondaryWeapon;

    public UnityEvent<WeaponDefinition> OnWeaponChanged = new UnityEvent<WeaponDefinition>();

    private void Awake()
    {
        if (weaponController == null)
        {
            weaponController = GetComponent<WeaponController>();
        }
    }

    private void Start()
    {
        InitializeFromLoadout();
    }

    public void InitializeFromLoadout()
    {
        if (loadoutDefinition == null)
        {
            Debug.LogWarning($"[LoadoutController] No LoadoutDefinition assigned on {gameObject.name}");
            return;
        }

        // Set up starting weapons
        primaryWeapon = loadoutDefinition.StartingPrimary;
        secondaryWeapon = loadoutDefinition.StartingSecondary;

        // Determine starting active slot
        if (loadoutDefinition.StartWithSecondaryActive || primaryWeapon == null)
        {
            activeSlot = WeaponSlot.Secondary;
        }
        else
        {
            activeSlot = WeaponSlot.Primary;
        }

        // Equip the starting weapon
        UpdateWeaponController();
    }

    public bool EquipPrimary(WeaponDefinition weapon, bool makeActive = true)
    {
        if (weapon == null)
        {
            Debug.LogWarning($"[LoadoutController] Cannot equip null primary weapon on {gameObject.name}");
            return false;
        }

        if (weapon.slot != WeaponSlot.Primary)
        {
            Debug.LogWarning($"[LoadoutController] Weapon '{weapon.displayName}' is not a Primary weapon (slot: {weapon.slot})");
            return false;
        }

        primaryWeapon = weapon;

        if (makeActive)
        {
            activeSlot = WeaponSlot.Primary;
            UpdateWeaponController();
        }

        return true;
    }

    public bool EquipSecondary(WeaponDefinition weapon, bool makeActive = false)
    {
        if (weapon == null)
        {
            Debug.LogWarning($"[LoadoutController] Cannot equip null secondary weapon on {gameObject.name}");
            return false;
        }

        if (weapon.slot != WeaponSlot.Secondary)
        {
            Debug.LogWarning($"[LoadoutController] Weapon '{weapon.displayName}' is not a Secondary weapon (slot: {weapon.slot})");
            return false;
        }

        secondaryWeapon = weapon;

        if (makeActive)
        {
            activeSlot = WeaponSlot.Secondary;
            UpdateWeaponController();
        }

        return true;
    }

    public bool SetActiveSlot(WeaponSlot slot)
    {
        // Validate the slot has a weapon
        WeaponDefinition targetWeapon = GetWeaponAtSlot(slot);
        if (targetWeapon == null)
        {
            Debug.LogWarning($"[LoadoutController] Cannot switch to {slot} slot - no weapon available");
            return false;
        }

        activeSlot = slot;
        UpdateWeaponController();
        return true;
    }

    public WeaponDefinition GetActiveWeapon()
    {
        return GetWeaponAtSlot(activeSlot);
    }

    private WeaponDefinition GetWeaponAtSlot(WeaponSlot slot)
    {
        switch (slot)
        {
            case WeaponSlot.Primary:
                return primaryWeapon;
            case WeaponSlot.Secondary:
                return secondaryWeapon;
            default:
                return null;
        }
    }

    private void UpdateWeaponController()
    {
        WeaponDefinition activeWeapon = GetActiveWeapon();

        if (weaponController != null)
        {
            weaponController.SetWeapon(activeWeapon);
        }

        OnWeaponChanged.Invoke(activeWeapon);
    }

    // Getters for debugging/UI
    public WeaponDefinition GetPrimaryWeapon() => primaryWeapon;
    public WeaponDefinition GetSecondaryWeapon() => secondaryWeapon;
    public WeaponSlot GetActiveSlot() => activeSlot;
}
