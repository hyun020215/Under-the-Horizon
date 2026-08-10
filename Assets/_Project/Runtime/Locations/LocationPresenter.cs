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
    [SerializeField]
    private AmbientParticleProfile defaultAmbientParticles;
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
                locationState?.AmbientParticles ?? defaultAmbientParticles);
        }
        if (location != null)
            state?.SetCurrentLocation(location.Id);
        return Task.CompletedTask;
    }
}
