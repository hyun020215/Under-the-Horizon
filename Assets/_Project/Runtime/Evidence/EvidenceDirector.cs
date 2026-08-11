using System;
using UnityEngine;

public sealed class EvidenceDirector : MonoBehaviour
{
    [SerializeField]
    private GameStateStore state;

    [SerializeField]
    private EvidenceDatabase database;
    public EvidenceInventory Inventory { get; private set; }
    public event Action<EvidenceDefinition> EvidenceDiscovered;

    private void Awake() => Inventory = new EvidenceInventory(state, database);

    private void OnEnable()
    {
        if (state != null)
            state.EvidenceAdded += OnEvidenceAdded;
    }

    private void OnDisable()
    {
        if (state != null)
            state.EvidenceAdded -= OnEvidenceAdded;
    }

    private void OnEvidenceAdded(string id)
    {
        EvidenceDefinition definition = database?.Find(id);
        if (definition != null)
            EvidenceDiscovered?.Invoke(definition);
    }
}
