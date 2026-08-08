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
        }
        if (location != null)
            state?.SetCurrentLocation(location.Id);
        return Task.CompletedTask;
    }
}
