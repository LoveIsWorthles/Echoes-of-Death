using System;
using UnityEngine;

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

    public event Action<WeaponDefinition> WeaponChanged;

    public WeaponDefinition PrimaryWeapon => primaryWeapon;
    public WeaponDefinition SecondaryWeapon => secondaryWeapon;
    public WeaponSlot ActiveSlot => activeSlot;

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
            Debug.LogWarning($"[LoadoutController] No LoadoutDefinition assigned on {gameObject.name}", this);
            return;
        }

        primaryWeapon = loadoutDefinition.StartingPrimary;
        secondaryWeapon = loadoutDefinition.StartingSecondary;

        if (loadoutDefinition.StartWithSecondaryActive || primaryWeapon == null)
        {
            activeSlot = WeaponSlot.Secondary;
        }
        else
        {
            activeSlot = WeaponSlot.Primary;
        }

        UpdateWeaponController();
    }

    public bool EquipPrimary(WeaponDefinition weapon, bool makeActive = true)
    {
        if (!ValidateForSlot(weapon, WeaponSlot.Primary))
        {
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
        if (!ValidateForSlot(weapon, WeaponSlot.Secondary))
        {
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
        if (GetWeaponAtSlot(slot) == null)
        {
            Debug.LogWarning($"[LoadoutController] Cannot switch to {slot} slot - no weapon available", this);
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

    private bool ValidateForSlot(WeaponDefinition weapon, WeaponSlot expectedSlot)
    {
        if (weapon == null)
        {
            Debug.LogWarning($"[LoadoutController] Cannot equip null {expectedSlot} weapon on {gameObject.name}", this);
            return false;
        }

        if (weapon.slot != expectedSlot)
        {
            Debug.LogWarning($"[LoadoutController] Weapon '{weapon.displayName}' is not a {expectedSlot} weapon (slot: {weapon.slot})", this);
            return false;
        }

        return true;
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

        WeaponChanged?.Invoke(activeWeapon);
    }
}
