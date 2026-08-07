using System.Collections.Generic;

public sealed class EvidenceInventory
{
    private readonly GameStateStore state;
    private readonly EvidenceDatabase database;

    public EvidenceInventory(GameStateStore state, EvidenceDatabase database)
    {
        this.state = state;
        this.database = database;
    }

    public bool Add(string id) => state.AddEvidence(id);

    public bool Has(string id) => state.HasEvidence(id);

    public IEnumerable<EvidenceDefinition> Discovered
    {
        get
        {
            foreach (string id in state.State.discoveredEvidence)
            {
                var item = database.Find(id);
                if (item != null)
                    yield return item;
            }
        }
    }
}
