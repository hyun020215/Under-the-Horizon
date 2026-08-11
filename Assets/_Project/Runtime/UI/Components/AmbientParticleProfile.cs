using UnityEngine;

[CreateAssetMenu(fileName = "UI_AMBIENCE_", menuName = "Under The Horizon/UI/Ambient Particles")]
public sealed class AmbientParticleProfile : ScriptableObject
{
    [SerializeField, Range(0, 64)] private int count = 18;
    [SerializeField] private Color tint = new(1f, .88f, .62f, .36f);
    [SerializeField] private Vector2 sizeRange = new(4f, 13f);
    [SerializeField] private Vector2 speedRange = new(4f, 10f);
    [SerializeField] private Vector2 alphaRange = new(.15f, .6f);
    [SerializeField, Range(0f, .25f)] private float sway = .06f;
    [Header("Optional layered atmosphere")]
    [SerializeField] private Sprite lightShaftSprite;
    [SerializeField] private Material lightShaftMaterial;
    [SerializeField, Range(0f, 1f)] private float lightShaftOpacity;
    [SerializeField, Range(0f, .1f)] private float lightShaftDrift = .015f;
    [SerializeField, Range(0f, 1f)] private float waterShimmerOpacity;
    [SerializeField, Min(.05f)] private float waterShimmerCycle = 4f;

    public int Count => count;
    public Color Tint => tint;
    public Vector2 SizeRange => sizeRange;
    public Vector2 SpeedRange => speedRange;
    public Vector2 AlphaRange => alphaRange;
    public float Sway => sway;
    public Sprite LightShaftSprite => lightShaftSprite;
    public Material LightShaftMaterial => lightShaftMaterial;
    public float LightShaftOpacity => lightShaftOpacity;
    public float LightShaftDrift => lightShaftDrift;
    public float WaterShimmerOpacity => waterShimmerOpacity;
    public float WaterShimmerCycle => waterShimmerCycle;
}
