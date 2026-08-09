using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class CharacterStage : MonoBehaviour
{
    [SerializeField]
    private CharacterView prefab;

    [SerializeField]
    private RectTransform root;

    [SerializeField]
    private InteractionDirector interactions;

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
