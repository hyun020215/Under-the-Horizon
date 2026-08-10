using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class LocationPresenter : MonoBehaviour
{
    [SerializeField]
    private Image background;

    [SerializeField]
    private AspectRatioFitter backgroundAspect;

    [SerializeField]
    private GameStateStore state;
    private AmbientParticleOverlay particles;
    public LocationDefinition Current { get; private set; }

    public Task ApplyAsync(LocationDefinition location, LocationStateDefinition locationState)
    {
        Current = location;
        if (background != null)
        {
            background.sprite =
                locationState != null && locationState.Background != null
                    ? locationState.Background
                    : location?.DefaultBackground;
            background.color = locationState != null ? locationState.Tint : Color.white;
            if (backgroundAspect != null && background.sprite != null)
                backgroundAspect.aspectRatio = background.sprite.rect.width
                    / background.sprite.rect.height;
            particles ??= background.GetComponent<AmbientParticleOverlay>()
                ?? background.gameObject.AddComponent<AmbientParticleOverlay>();
            particles.Initialize(background.rectTransform,
                new Color(1f, 0.86f, 0.58f, 0.32f));
            if (locationState != null)
            {
                Color particleTint = Color.Lerp(
                    new Color(0.62f, 0.78f, 1f, 0.28f),
                    new Color(1f, 0.82f, 0.52f, 0.36f),
                    Mathf.Clamp01(locationState.Tint.r));
                particles.SetTint(particleTint);
            }
        }
        if (location != null)
            state?.SetCurrentLocation(location.Id);
        return Task.CompletedTask;
    }
}
