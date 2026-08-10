using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class AmbientParticleOverlay : MonoBehaviour
{
    private RectTransform[] particles;
    private Image[] images;
    private RectTransform host;
    private float elapsed;
    private AmbientParticleProfile profile;
    private static Sprite glowSprite;

    public void Initialize(RectTransform target, AmbientParticleProfile settings)
    {
        host = target;
        if (profile != settings)
        {
            profile = settings;
            Rebuild();
        }
    }

    private void Awake()
    {
        host ??= transform as RectTransform;
        if (host != null && profile != null && particles == null)
            Build();
    }

    private void Rebuild()
    {
        if (particles != null)
            foreach (RectTransform particle in particles)
                if (particle != null)
                    Destroy(particle.gameObject);
        particles = null;
        images = null;
        if (host != null && profile != null)
            Build();
    }

    private void Build()
    {
        EnsureGlowSprite();
        particles = new RectTransform[profile.Count];
        images = new Image[profile.Count];
        for (int index = 0; index < profile.Count; index++)
        {
            GameObject particle = new(
                $"Ambient Particle {index + 1}",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            particle.transform.SetParent(host, false);
            RectTransform rect = particle.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            float size = Mathf.Lerp(profile.SizeRange.x, profile.SizeRange.y, Hash01(index, 0));
            rect.sizeDelta = new Vector2(size, size);
            Image image = particle.GetComponent<Image>();
            image.sprite = glowSprite;
            image.raycastTarget = false;
            particles[index] = rect;
            images[index] = image;
        }
    }

    private void Update()
    {
        if (host == null || profile == null || particles == null ||
            particles.Length == 0 || host.rect.width <= 0f)
            return;
        elapsed += Time.unscaledDeltaTime;
        Rect bounds = host.rect;
        for (int index = 0; index < particles.Length; index++)
        {
            float speed = Mathf.Lerp(profile.SpeedRange.x, profile.SpeedRange.y, Hash01(index, 1));
            float y = bounds.yMin + Mathf.Repeat(
                Hash01(index, 2) * bounds.height + elapsed * speed,
                bounds.height);
            float sway = Mathf.Sin(elapsed * Mathf.Lerp(0.3f, 0.7f, Hash01(index, 3))
                + Hash01(index, 4) * Mathf.PI * 2f);
            float x = bounds.xMin + Mathf.Repeat(
                Hash01(index, 5) * bounds.width + sway * bounds.width * profile.Sway,
                bounds.width);
            particles[index].anchoredPosition = new Vector2(x, y);
            Color color = profile.Tint;
            color.a *= Mathf.Lerp(profile.AlphaRange.x, profile.AlphaRange.y,
                (Mathf.Sin(elapsed * 1.2f + index) + 1f) * 0.5f);
            images[index].color = color;
        }
    }

    private static void EnsureGlowSprite()
    {
        if (glowSprite != null)
            return;
        const int size = 32;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
        {
            name = "UI Ambient Glow",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };
        Color[] colors = new Color[size * size];
        Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float distance = Vector2.Distance(new Vector2(x, y), center) / center.x;
            float alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 2.2f);
            colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
        }
        texture.SetPixels(colors);
        texture.Apply(false, true);
        glowSprite = Sprite.Create(texture, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f);
        glowSprite.name = "UI Ambient Glow";
    }

    private static float Hash01(int seed, int channel)
    {
        unchecked
        {
            int hash = (seed + 104729) * 374761393 + channel * 668265263;
            hash = (hash ^ (hash >> 13)) * 1274126177;
            hash ^= hash >> 16;
            return (hash & 0x7fffffff) / (float)int.MaxValue;
        }
    }
}
