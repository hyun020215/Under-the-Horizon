using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class CharacterStage : MonoBehaviour
{
    [SerializeField] private CharacterView prefab;
    [SerializeField] private RectTransform root;
    private readonly List<CharacterView> views = new();

    public Task ApplyAsync(CharacterPlacementSet set)
    {
        Clear();
        if (set?.Placements != null && prefab != null)
        {
            foreach (CharacterPlacement placement in set.Placements)
            {
                CharacterView view = Instantiate(prefab, root);
                view.Apply(placement);
                views.Add(view);
            }
        }
        return Task.CompletedTask;
    }

    public void Clear()
    {
        foreach (CharacterView view in views) if (view != null) Destroy(view.gameObject);
        views.Clear();
    }
}
