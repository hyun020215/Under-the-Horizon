using System;
using UnityEngine;

public sealed class SaveCheckpoint : MonoBehaviour
{
    [SerializeField]
    private GameStateStore stateStore;

    [SerializeField]
    private StorySceneDirector storyScenes;

    [SerializeField]
    private int slot;

    private bool isBound;

    private void OnEnable()
    {
        if (storyScenes != null)
            storyScenes.Entered += HandleStorySceneEntered;
    }

    private void OnDisable()
    {
        if (storyScenes != null)
            storyScenes.Entered -= HandleStorySceneEntered;
    }

    public void Bind(SaveSlot selectedSlot)
    {
        slot = selectedSlot.Index;
        isBound = true;
    }

    public void Capture()
    {
        if (!isBound || stateStore == null || !TryResolveSaveService(out SaveService saves))
            return;

        try
        {
            saves.Save(new SaveSlot(slot), stateStore.State);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }

    private void HandleStorySceneEntered(StorySceneDefinition _) => Capture();

    private static bool TryResolveSaveService(out SaveService saves)
    {
        if (AppContext.Services != null
            && AppContext.Services.TryGet(out saves))
        {
            return true;
        }

        saves = null;
        return false;
    }
}
