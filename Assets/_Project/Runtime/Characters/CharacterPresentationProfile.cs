using UnityEngine;

[CreateAssetMenu(fileName = "CHR_PRESENTATION_", menuName = "Under The Horizon/Characters/Presentation")]
public sealed class CharacterPresentationProfile : ScriptableObject
{
    [SerializeField] private Vector2 breathingCycleRange = new(3.15f, 4.05f);
    [SerializeField] private Vector2 swayCycleRange = new(4.3f, 5.3f);
    [SerializeField] private float verticalMotion = 1.5f;
    [SerializeField] private float breathingScale = .006f;
    [SerializeField] private float swayDegrees = .65f;
    [SerializeField] private float blendDuration = .35f;
    [SerializeField] private Color silhouetteColor = new(.015f, .02f, .03f, .58f);
    [SerializeField] private Vector2 silhouetteDistance = new(7f, -5f);
    [SerializeField] private Vector2 groundShadowSize = new(330f, 82f);
    [SerializeField] private Vector2 groundShadowOffset = new(0f, 10f);
    [SerializeField] private Color groundShadowColor = new(.005f, .01f, .018f, .46f);

    public Vector2 BreathingCycleRange => breathingCycleRange;
    public Vector2 SwayCycleRange => swayCycleRange;
    public float VerticalMotion => verticalMotion;
    public float BreathingScale => breathingScale;
    public float SwayDegrees => swayDegrees;
    public float BlendDuration => blendDuration;
    public Color SilhouetteColor => silhouetteColor;
    public Vector2 SilhouetteDistance => silhouetteDistance;
    public Vector2 GroundShadowSize => groundShadowSize;
    public Vector2 GroundShadowOffset => groundShadowOffset;
    public Color GroundShadowColor => groundShadowColor;
}
