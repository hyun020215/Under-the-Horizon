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

    private async void Start()
    {
        if (flow == null)
            return;

        try
        {
            if (screens != null && titleScreen != null)
            {
                await screens.OpenAsync(ScreenId.Title);
                await titleScreen.WaitForStartAsync();
            }

            string sceneId = firstStorySceneId;
            if (screens != null && saveSlotScreen != null)
            {
                await screens.OpenAsync(ScreenId.SaveSlot);
                SaveSlot slot = await saveSlotScreen.WaitForSelectionAsync();
                var saves = new SaveService();
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
}
