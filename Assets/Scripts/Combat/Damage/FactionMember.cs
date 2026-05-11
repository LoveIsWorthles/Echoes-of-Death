using UnityEngine;

[DisallowMultipleComponent]
public class FactionMember : MonoBehaviour
{
    [SerializeField]
    private Faction faction = Faction.Neutral;

    public Faction Faction => faction;
}