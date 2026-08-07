using UnityEngine;

[CreateAssetMenu(
    fileName = "SET_CHARACTERS",
    menuName = "Under The Horizon/Characters/Placement Set"
)]
public sealed class CharacterPlacementSet : ScriptableObject
{
    [SerializeField]
    private CharacterPlacement[] placements;
    public CharacterPlacement[] Placements => placements;
}
