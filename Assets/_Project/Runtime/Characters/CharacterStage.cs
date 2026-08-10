using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class CharacterStage : MonoBehaviour
{
    [SerializeField]
    private CharacterView prefab;

    [SerializeField]
    private RectTransform root;

    [SerializeField]
    private InteractionDirector interactions;
    [SerializeField]
    private CharacterPresentationProfile defaultPresentation;

    private readonly List<CharacterView> views = new();
    private readonly List<GameObject> shadows = new();
    private static Sprite groundShadowSprite;

    public Task ApplyAsync(CharacterPlacementSet set)
    {
        Clear();
        if (set?.Placements != null && prefab != null)
        {
            foreach (CharacterPlacement placement in set.Placements)
            {
                shadows.Add(CreateGroundShadow(placement));
                CharacterView view = Instantiate(prefab, root);
                view.ConfigurePresentation(defaultPresentation);
                view.Apply(placement);
                view.Clicked += OnCharacterClicked;
                views.Add(view);
            }
        }
        return Task.CompletedTask;
    }

    public void Clear()
    {
        foreach (CharacterView view in views)
            if (view != null)
            {
                view.Clicked -= OnCharacterClicked;
                Destroy(view.gameObject);
            }
        views.Clear();
        foreach (GameObject shadow in shadows)
            if (shadow != null)
                Destroy(shadow);
        shadows.Clear();
    }

    private GameObject CreateGroundShadow(CharacterPlacement placement)
    {
        EnsureGroundShadowSprite();
        GameObject shadow = new(
            $"{placement.character?.Id ?? "Character"} Ground Shadow",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        shadow.transform.SetParent(root, false);
        RectTransform rect = shadow.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(
            placement.normalizedX, placement.normalizedY);
        rect.pivot = new Vector2(0.5f, 0.5f);
        CharacterPresentationProfile profile =
            placement.character?.PresentationOverride ?? defaultPresentation;
        if (profile == null)
            return shadow;
        rect.anchoredPosition = profile.GroundShadowOffset;
        float scale = placement.scale <= 0f ? 1f : placement.scale;
        rect.sizeDelta = profile.GroundShadowSize * scale;
        Image image = shadow.GetComponent<Image>();
        image.sprite = groundShadowSprite;
        image.color = profile.GroundShadowColor;
        image.raycastTarget = false;
        return shadow;
    }

    private static void EnsureGroundShadowSprite()
    {
        if (groundShadowSprite != null)
            return;
        const int width = 128;
        const int height = 32;
        Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
        {
            name = "Character Ground Shadow",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };
        Color[] colors = new Color[width * height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float nx = (x - width * 0.5f) / (width * 0.5f);
            float ny = (y - height * 0.5f) / (height * 0.5f);
            float alpha = Mathf.Pow(Mathf.Clamp01(1f - nx * nx - ny * ny), 2f);
            colors[y * width + x] = new Color(1f, 1f, 1f, alpha);
        }
        texture.SetPixels(colors);
        texture.Apply(false, true);
        groundShadowSprite = Sprite.Create(texture,
            new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private async void OnCharacterClicked(CharacterView view)
    {
        if (interactions == null)
            return;

        try
        {
            await interactions.ExecuteFirstAvailableAsync(
                InteractionType.Character,
                view.Definition?.Id);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception, view);
        }
    }
}
