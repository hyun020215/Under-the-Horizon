using UnityEngine;
using UnityEngine.UI;

public sealed class CharacterView : MonoBehaviour
{
    [SerializeField]
    private Image image;
    public CharacterDefinition Definition { get; private set; }

    public void Apply(CharacterPlacement placement)
    {
        Definition = placement.character;
        if (image != null)
            image.sprite = Definition?.Resolve(placement.pose, placement.expression);
        RectTransform rect = transform as RectTransform;
        if (rect != null)
            rect.anchorMin = rect.anchorMax = new Vector2(
                placement.normalizedX,
                placement.normalizedY
            );
        transform.localScale = Vector3.one * (placement.scale <= 0 ? 1 : placement.scale);
        gameObject.SetActive(Definition != null);
    }
}
