using System;
using UnityEngine;

public sealed class GameStartup : MonoBehaviour
{
    [SerializeField]
    private GameFlowController flow;

    [SerializeField]
    private string firstStorySceneId = "P-01";

    [SerializeField]
    private ScreenRouter screens;

    [SerializeField]
    private TitleScreen titleScreen;

    [SerializeField]
    private SaveSlotScreen saveSlotScreen;

    [SerializeField]
    private GameStateStore state;

    [SerializeField]
    private AudioDirector audioDirector;

    [SerializeField]
    private AudioCueProfile titleAudio;

    private async void Start()
    {
        if (flow == null)
            return;

        try
        {
            if (screens != null && titleScreen != null)
            {
                audioDirector?.Apply(titleAudio);
                await screens.OpenAsync(ScreenId.Title);
                await titleScreen.WaitForStartAsync();
            }

            string sceneId = ResolveFirstStorySceneId();
            if (screens != null && saveSlotScreen != null)
            {
                await screens.OpenAsync(ScreenId.SaveSlot);
                SaveSlot slot = await saveSlotScreen.WaitForSelectionAsync();
                SaveService saves = ResolveSaveService();
                if (saves.Exists(slot))
                {
                    state?.Replace(saves.Load(slot));
                    if (!string.IsNullOrWhiteSpace(state?.State.currentStorySceneId))
                        sceneId = state.State.currentStorySceneId;
                }
                else
                {
                    state?.Replace(new GameState());
                }
            }

            await flow.StartAsync(sceneId);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }

    private string ResolveFirstStorySceneId()
    {
        if (AppContext.Services != null
            && AppContext.Services.TryGet(out GameDefinition game)
            && !string.IsNullOrWhiteSpace(game.FirstStorySceneId))
        {
            return game.FirstStorySceneId;
        }

        return firstStorySceneId;
    }

    private static SaveService ResolveSaveService()
    {
        if (AppContext.Services != null
            && AppContext.Services.TryGet(out SaveService saves))
        {
            return saves;
        }

        return new SaveService();
    }
}
