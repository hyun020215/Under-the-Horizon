using UnityEngine;

[CreateAssetMenu(
    fileName = "SET_CHARACTERS",
    menuName = "Under The Horizon/Characters/Placement Set"
)]
public sealed class CharacterPlacementSet : ScriptableObject
{
    [SerializeField]
    private CharacterPlacementSpace placementSpace;

    [SerializeField]
    private CharacterPlacement[] placements;

    public CharacterPlacementSpace PlacementSpace => placementSpace;
    public CharacterPlacement[] Placements => placements;
}
