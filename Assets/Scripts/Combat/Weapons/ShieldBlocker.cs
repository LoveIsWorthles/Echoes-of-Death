using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ShieldBlocker : MonoBehaviour, IDamageBlocker
{
    [Header("References")]
    [SerializeField]
    private LoadoutController loadout;

    [Header("Fallback Config")]
    [SerializeField, Min(1)]
    private int fallbackMaxCharges = 5;

    [SerializeField, Range(0f, 360f)]
    private float fallbackFrontBlockAngle = 120f;

    [SerializeField, Range(0f, 360f)]
    private float fallbackBackBlockAngle = 60f;

    private WeaponDefinition shieldDefinition;
    private int currentCharges;

    public event Action<DamageInfo> ShieldBlocked;
    public event Action ShieldBroken;
    public event Action<int, int> ShieldChargesChanged;

    public bool HasShield => shieldDefinition != null;
    public int CurrentCharges => currentCharges;
    public int MaxCharges => shieldDefinition != null ? shieldDefinition.shieldMaxCharges : fallbackMaxCharges;

    private float FrontBlockAngle => shieldDefinition != null ? shieldDefinition.frontBlockAngle : fallbackFrontBlockAngle;
    private float BackBlockAngle => shieldDefinition != null ? shieldDefinition.backBlockAngle : fallbackBackBlockAngle;

    private void Awake()
    {
        if (loadout == null)
        {
            loadout = GetComponent<LoadoutController>();
        }
    }

    public void ReplaceWithFreshShield(WeaponDefinition definition)
    {
        shieldDefinition = definition;
        currentCharges = MaxCharges;
        ShieldChargesChanged?.Invoke(currentCharges, MaxCharges);
    }

    public void ClearShield()
    {
        shieldDefinition = null;
        currentCharges = 0;
        ShieldChargesChanged?.Invoke(currentCharges, MaxCharges);
    }

    public bool BlocksDamage(DamageInfo damageInfo)
    {
        if (shieldDefinition == null || currentCharges <= 0)
        {
            return false;
        }

        if (loadout == null || loadout.PrimaryWeapon != shieldDefinition)
        {
            return false;
        }

        bool active = loadout.ActiveSlot == WeaponSlot.Primary;

        if (!IsWithinBlockAngle(damageInfo.IncomingDirection, active))
        {
            return false;
        }

        ConsumeCharge(damageInfo);
        return true;
    }

    private bool IsWithinBlockAngle(Vector3 incomingDirection, bool shieldActive)
    {
        Vector3 toAttacker = new Vector3(-incomingDirection.x, 0f, -incomingDirection.z);
        if (toAttacker.sqrMagnitude < 0.0001f)
        {
            return false;
        }
        toAttacker.Normalize();

        Vector3 forward = transform.forward;
        Vector3 forwardXZ = new Vector3(forward.x, 0f, forward.z);
        if (forwardXZ.sqrMagnitude < 0.0001f)
        {
            return false;
        }
        forwardXZ.Normalize();

        if (shieldActive)
        {
            return Vector3.Angle(forwardXZ, toAttacker) <= FrontBlockAngle * 0.5f;
        }

        return Vector3.Angle(-forwardXZ, toAttacker) <= BackBlockAngle * 0.5f;
    }

    private void ConsumeCharge(DamageInfo damageInfo)
    {
        currentCharges--;
        ShieldBlocked?.Invoke(damageInfo);
        ShieldChargesChanged?.Invoke(currentCharges, MaxCharges);

        if (currentCharges <= 0)
        {
            ShieldBroken?.Invoke();
        }
    }
}
