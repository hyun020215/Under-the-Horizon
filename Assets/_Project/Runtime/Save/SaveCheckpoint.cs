using UnityEngine;
public sealed class SaveCheckpoint : MonoBehaviour
{
    [SerializeField] private GameStateStore stateStore;
    [SerializeField] private int slot;
    public void Capture() { if (stateStore != null) new SaveService().Save(new SaveSlot(slot), stateStore.State); }
}
