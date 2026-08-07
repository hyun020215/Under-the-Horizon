using UnityEngine;

public sealed class EvidenceDirector : MonoBehaviour
{
    [SerializeField]
    private GameStateStore state;

    [SerializeField]
    private EvidenceDatabase database;
    public EvidenceInventory Inventory { get; private set; }

    private void Awake() => Inventory = new EvidenceInventory(state, database);
}
