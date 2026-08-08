using UnityEngine;
using UnityEngine.UI;

public sealed class PersistentHud : MonoBehaviour
{
    [SerializeField]
    private ScreenRouter screens;

    [SerializeField]
    private Button mapButton;

    [SerializeField]
    private Button recordButton;

    private void Awake()
    {
        mapButton?.onClick.AddListener(() => Open(ScreenId.Map));
        recordButton?.onClick.AddListener(() => Open(ScreenId.InvestigationRecord));
        if (screens != null)
            screens.Opened += OnScreenOpened;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (screens != null)
            screens.Opened -= OnScreenOpened;
    }

    private void OnScreenOpened(ScreenId id)
    {
        bool visible = id != ScreenId.Title
            && id != ScreenId.SaveSlot
            && id != ScreenId.Ending
            && id != ScreenId.Credits;
        gameObject.SetActive(visible);
    }

    private async void Open(ScreenId id)
    {
        if (screens == null)
            return;
        try
        {
            await screens.OpenAsync(id);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }
}
