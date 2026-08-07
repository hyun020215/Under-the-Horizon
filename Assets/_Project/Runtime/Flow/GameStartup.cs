using System;
using UnityEngine;

public sealed class GameStartup : MonoBehaviour
{
    [SerializeField]
    private GameFlowController flow;

    [SerializeField]
    private string firstStorySceneId = "P-01";

    private async void Start()
    {
        if (flow == null)
            return;

        try
        {
            await flow.StartAsync(firstStorySceneId);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }
}
