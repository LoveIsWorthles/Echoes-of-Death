using System;
using UnityEngine;

public class LoadoutController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private WeaponController weaponController;

    [SerializeField]
    private ShieldBlocker shieldBlocker;

    [SerializeField]
    private LoadoutDefinition loadoutDefinition;

    [Header("State")]
    [SerializeField]
    private WeaponSlot activeSlot = WeaponSlot.Secondary;

    private WeaponDefinition primaryWeapon;
    private ShieldDefinition primaryShield;
    private WeaponDefinition secondaryWeapon;

    public event Action<WeaponDefinition> WeaponChanged;
    public event Action<WeaponSlot> ActiveSlotChanged;
    public event Action<ShieldDefinition> ShieldEquipped;
    public event Action ShieldRemoved;

    public WeaponDefinition PrimaryWeapon => primaryWeapon;
    public ShieldDefinition PrimaryShield => primaryShield;
    public WeaponDefinition SecondaryWeapon => secondaryWeapon;
    public WeaponSlot ActiveSlot => activeSlot;
    public bool HasPrimaryShield => primaryShield != null;
    public bool HasPrimaryWeapon => primaryWeapon != null;

    private void Awake()
    {
        if (weaponController == null)
        {
            weaponController = GetComponent<WeaponController>();
        }

        if (shieldBlocker == null)
        {
            shieldBlocker = GetComponent<ShieldBlocker>();
        }
    }

    private void OnEnable()
    {
        if (shieldBlocker != null)
        {
            shieldBlocker.ShieldBroken += HandleShieldBroken;
        }
    }

    private void OnDisable()
    {
        if (shieldBlocker != null)
        {
            shieldBlocker.ShieldBroken -= HandleShieldBroken;
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

        primaryShield = loadoutDefinition.StartingShield;
        primaryWeapon = primaryShield != null ? null : loadoutDefinition.StartingPrimary;
        secondaryWeapon = loadoutDefinition.StartingSecondary;

        if (primaryShield != null && loadoutDefinition.StartingPrimary != null)
        {
            Debug.LogWarning($"[LoadoutController] LoadoutDefinition '{loadoutDefinition.name}' has both a starting weapon and a starting shield in the primary slot. The shield takes precedence.", loadoutDefinition);
        }

        if (primaryShield != null && shieldBlocker != null)
        {
            shieldBlocker.ReplaceWithFreshShield(primaryShield);
            ShieldEquipped?.Invoke(primaryShield);
        }

        bool hasPrimary = primaryWeapon != null || primaryShield != null;
        if (loadoutDefinition.StartWithSecondaryActive || !hasPrimary)
        {
            activeSlot = WeaponSlot.Secondary;
        }
        else
        {
            activeSlot = WeaponSlot.Primary;
        }

        UpdateWeaponController();
        ActiveSlotChanged?.Invoke(activeSlot);
    }

    public bool EquipPrimary(WeaponDefinition weapon, bool makeActive = true)
    {
        if (!ValidateForSlot(weapon, WeaponSlot.Primary))
        {
            return false;
        }

        ClearShieldInternal();
        primaryWeapon = weapon;

        if (makeActive)
        {
            SetActiveSlotInternal(WeaponSlot.Primary);
        }
        else
        {
            UpdateWeaponController();
        }

        return true;
    }

    public bool EquipShield(ShieldDefinition shield)
    {
        if (shield == null)
        {
            Debug.LogWarning($"[LoadoutController] Cannot equip null shield on {gameObject.name}", this);
            return false;
        }

        if (shieldBlocker == null)
        {
            Debug.LogWarning($"[LoadoutController] Cannot equip shield - no ShieldBlocker assigned on {gameObject.name}", this);
            return false;
        }

        primaryWeapon = null;
        primaryShield = shield;
        shieldBlocker.ReplaceWithFreshShield(shield);

        ShieldEquipped?.Invoke(shield);
        SetActiveSlotInternal(WeaponSlot.Primary);
        return true;
    }

    public bool RefillShield()
    {
        if (primaryShield == null || shieldBlocker == null)
        {
            return false;
        }

        if (shieldBlocker.CurrentCharges >= shieldBlocker.MaxCharges)
        {
            return false;
        }

        shieldBlocker.RefillCharges();
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
            SetActiveSlotInternal(WeaponSlot.Secondary);
        }
        else
        {
            UpdateWeaponController();
        }

        return true;
    }

    public bool SetActiveSlot(WeaponSlot slot)
    {
        if (!HasContentAt(slot))
        {
            Debug.LogWarning($"[LoadoutController] Cannot switch to {slot} slot - nothing equipped there", this);
            return false;
        }

        SetActiveSlotInternal(slot);
        return true;
    }

    public WeaponDefinition GetActiveWeapon()
    {
        if (activeSlot == WeaponSlot.Primary && primaryShield != null)
        {
            return null;
        }
        return GetWeaponAtSlot(activeSlot);
    }

    private void HandleShieldBroken()
    {
        if (primaryShield == null)
        {
            return;
        }

        ClearShieldInternal();

        if (secondaryWeapon != null)
        {
            SetActiveSlotInternal(WeaponSlot.Secondary);
        }
        else
        {
            UpdateWeaponController();
        }
    }

    private void ClearShieldInternal()
    {
        if (primaryShield == null)
        {
            return;
        }

        primaryShield = null;
        if (shieldBlocker != null)
        {
            shieldBlocker.ClearShield();
        }
        ShieldRemoved?.Invoke();
    }

    private void SetActiveSlotInternal(WeaponSlot slot)
    {
        bool changed = activeSlot != slot;
        activeSlot = slot;
        UpdateWeaponController();
        if (changed)
        {
            ActiveSlotChanged?.Invoke(activeSlot);
        }
    }

    private bool HasContentAt(WeaponSlot slot)
    {
        switch (slot)
        {
            case WeaponSlot.Primary:
                return primaryWeapon != null || primaryShield != null;
            case WeaponSlot.Secondary:
                return secondaryWeapon != null;
            default:
                return false;
        }
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
