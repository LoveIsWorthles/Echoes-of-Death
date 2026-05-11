using System;
using UnityEngine;

public class GrenadeSlotController : MonoBehaviour
{
    [Header("Definitions")]
    [SerializeField]
    private GrenadeDefinition fragDefinition;

    [SerializeField]
    private GrenadeDefinition flashbangDefinition;

    [Header("State")]
    [SerializeField]
    private GrenadeType selectedGrenadeType = GrenadeType.Frag;

    [SerializeField, Min(0)]
    private int fragCount;

    [SerializeField, Min(0)]
    private int flashbangCount;

    [Header("Behavior")]
    [SerializeField]
    private bool autoSelectFirstAvailableGrenade = true;

    public event Action<GrenadeType, int> GrenadeCountsChanged;
    public event Action<GrenadeType> SelectedGrenadeChanged;
    public event Action<GrenadeDefinition, GrenadeType, Vector3> GrenadeThrown;

    public GrenadeType SelectedGrenadeType => selectedGrenadeType;

    private void Awake()
    {
        NormalizeSelectedType();
    }

    public void Initialize(GrenadeDefinition frag, int fragAmount, GrenadeDefinition flashbang, int flashbangAmount)
    {
        fragDefinition = frag;
        flashbangDefinition = flashbang;

        fragCount = ClampToCarryLimit(fragDefinition, fragAmount);
        flashbangCount = ClampToCarryLimit(flashbangDefinition, flashbangAmount);

        if (autoSelectFirstAvailableGrenade)
        {
            NormalizeSelectedType();
        }

        RaiseCountsChanged(GrenadeType.Frag);
        RaiseCountsChanged(GrenadeType.Flashbang);
        SelectedGrenadeChanged?.Invoke(selectedGrenadeType);
    }

    public void AddGrenades(GrenadeDefinition definition, int amount)
    {
        if (definition == null || amount <= 0)
        {
            return;
        }

        if (definition.grenadeType == GrenadeType.Frag)
        {
            fragDefinition = definition;
            fragCount = ClampToCarryLimit(definition, fragCount + amount);
            RaiseCountsChanged(GrenadeType.Frag);
        }
        else if (definition.grenadeType == GrenadeType.Flashbang)
        {
            flashbangDefinition = definition;
            flashbangCount = ClampToCarryLimit(definition, flashbangCount + amount);
            RaiseCountsChanged(GrenadeType.Flashbang);
        }

        NormalizeSelectedType();
    }

    public bool CanThrowSelected()
    {
        return GetSelectedCount() > 0 && GetSelectedDefinition() != null;
    }

    public bool TryThrowSelected(Vector3 targetWorldPosition)
    {
        GrenadeDefinition selectedDefinition = GetSelectedDefinition();
        if (selectedDefinition == null || GetSelectedCount() <= 0)
        {
            return false;
        }

        DecreaseSelectedCount();
        GrenadeThrown?.Invoke(selectedDefinition, selectedGrenadeType, targetWorldPosition);
        RaiseCountsChanged(selectedGrenadeType);
        NormalizeSelectedType();
        return true;
    }

    public bool SelectGrenadeType(GrenadeType type)
    {
        if (GetCount(type) <= 0)
        {
            return false;
        }

        if (selectedGrenadeType == type)
        {
            return true;
        }

        selectedGrenadeType = type;
        SelectedGrenadeChanged?.Invoke(selectedGrenadeType);
        return true;
    }

    public bool CycleGrenadeType()
    {
        GrenadeType nextType = selectedGrenadeType == GrenadeType.Frag ? GrenadeType.Flashbang : GrenadeType.Frag;

        if (GetCount(nextType) > 0)
        {
            return SelectGrenadeType(nextType);
        }

        if (GetCount(selectedGrenadeType) > 0)
        {
            return true;
        }

        GrenadeType fallbackType = GetCount(GrenadeType.Frag) > 0 ? GrenadeType.Frag : GrenadeType.Flashbang;
        return GetCount(fallbackType) > 0 && SelectGrenadeType(fallbackType);
    }

    public int GetCount(GrenadeType type)
    {
        return type switch
        {
            GrenadeType.Frag => fragCount,
            GrenadeType.Flashbang => flashbangCount,
            _ => 0
        };
    }

    private GrenadeDefinition GetSelectedDefinition()
    {
        return selectedGrenadeType == GrenadeType.Frag ? fragDefinition : flashbangDefinition;
    }

    private int GetSelectedCount()
    {
        return GetCount(selectedGrenadeType);
    }

    private void DecreaseSelectedCount()
    {
        if (selectedGrenadeType == GrenadeType.Frag)
        {
            fragCount = Mathf.Max(0, fragCount - 1);
        }
        else if (selectedGrenadeType == GrenadeType.Flashbang)
        {
            flashbangCount = Mathf.Max(0, flashbangCount - 1);
        }
    }

    private static int ClampToCarryLimit(GrenadeDefinition definition, int amount)
    {
        if (definition == null)
        {
            return 0;
        }

        return Mathf.Clamp(amount, 0, definition.maxCarryCount);
    }

    private void NormalizeSelectedType()
    {
        if (GetCount(selectedGrenadeType) > 0)
        {
            return;
        }

        if (GetCount(GrenadeType.Frag) > 0)
        {
            selectedGrenadeType = GrenadeType.Frag;
            SelectedGrenadeChanged?.Invoke(selectedGrenadeType);
            return;
        }

        if (GetCount(GrenadeType.Flashbang) > 0)
        {
            selectedGrenadeType = GrenadeType.Flashbang;
            SelectedGrenadeChanged?.Invoke(selectedGrenadeType);
        }
    }

    private void RaiseCountsChanged(GrenadeType type)
    {
        GrenadeCountsChanged?.Invoke(type, GetCount(type));
    }
}
