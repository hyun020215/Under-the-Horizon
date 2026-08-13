using UnityEngine;

public static class UiGlowSprite
{
    private static Sprite sprite;

    public static Sprite Get()
    {
        if (sprite != null)
            return sprite;

        const int size = 32;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
        {
            name = "UI Glow",
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
        sprite = Sprite.Create(texture, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f);
        sprite.name = "UI Glow";
        return sprite;
    }
}
