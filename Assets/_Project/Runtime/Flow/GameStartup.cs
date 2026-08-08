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

            await flow.StartAsync(firstStorySceneId);
            while (flow.CanAdvance)
                await flow.AdvanceAsync();

            if (screens != null)
                await screens.OpenAsync(ScreenId.Ending);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }
}
