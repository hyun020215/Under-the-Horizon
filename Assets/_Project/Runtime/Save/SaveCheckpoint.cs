using UnityEngine;

public sealed class SaveCheckpoint : MonoBehaviour
{
    [SerializeField]
    private GameStateStore stateStore;

    [SerializeField]
    private int slot;

    public void Capture()
    {
        if (stateStore != null)
            ResolveSaveService().Save(new SaveSlot(slot), stateStore.State);
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
