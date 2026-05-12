using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ShieldBlocker : MonoBehaviour, IDamageBlocker
{
    [Header("References")]
    [SerializeField]
    private LoadoutController loadout;

    private ShieldDefinition shieldDefinition;
    private int currentCharges;

    public event Action<DamageInfo> ShieldBlocked;
    public event Action ShieldBroken;
    public event Action<int, int> ShieldChargesChanged;

    public bool HasShield => shieldDefinition != null;
    public int CurrentCharges => currentCharges;
    public int MaxCharges => shieldDefinition != null ? shieldDefinition.maxCharges : 0;
    public ShieldDefinition Definition => shieldDefinition;

    private void Awake()
    {
        if (loadout == null)
        {
            loadout = GetComponent<LoadoutController>();
        }
    }

    public void ReplaceWithFreshShield(ShieldDefinition definition)
    {
        shieldDefinition = definition;
        currentCharges = MaxCharges;
        ShieldChargesChanged?.Invoke(currentCharges, MaxCharges);
    }

    public void RefillCharges()
    {
        if (shieldDefinition == null)
        {
            return;
        }

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

        if (damageInfo.DamageType != DamageType.Bullet)
        {
            return false;
        }

        if (loadout == null || loadout.PrimaryShield != shieldDefinition)
        {
            return false;
        }

        bool active = loadout.ActiveSlot == WeaponSlot.Primary;

        if (!IsWithinBlockAngle(damageInfo.IncomingDirection, active))
        {
            return false;
        }

        ConsumeCharges(damageInfo);
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

        float blockAngle = shieldActive ? shieldDefinition.frontBlockAngle : shieldDefinition.backBlockAngle;
        Vector3 facing = shieldActive ? forwardXZ : -forwardXZ;
        return Vector3.Angle(facing, toAttacker) <= blockAngle * 0.5f;
    }

    private void ConsumeCharges(DamageInfo damageInfo)
    {
        int cost = Mathf.Max(1, damageInfo.Amount);
        currentCharges = Mathf.Max(0, currentCharges - cost);

        ShieldBlocked?.Invoke(damageInfo);
        ShieldChargesChanged?.Invoke(currentCharges, MaxCharges);

        if (currentCharges <= 0)
        {
            ShieldBroken?.Invoke();
        }
    }
}
