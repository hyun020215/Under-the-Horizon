using UnityEngine;

[CreateAssetMenu(fileName = "CHR_", menuName = "Under The Horizon/Characters/Definition")]
public sealed class CharacterDefinition : ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private string displayName;

    [SerializeField]
    private Sprite portrait;

    [SerializeField]
    private CharacterVisualSet[] visuals;
    [SerializeField]
    private CharacterPresentationProfile presentationOverride;
    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Portrait => portrait;
    public CharacterPresentationProfile PresentationOverride => presentationOverride;

    public Sprite Resolve(CharacterPose pose, CharacterExpression expression)
    {
        if (visuals != null)
            foreach (var item in visuals)
                if (item != null && item.Pose == pose && item.Expression == expression)
                    return item.Sprite;
        return portrait;
    }
}
