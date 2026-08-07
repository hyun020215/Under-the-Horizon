using System;
using UnityEngine;

[Serializable]
public sealed class LocationExit
{
    [SerializeField]
    private string id;

    [SerializeField]
    private LocationDefinition destination;

    [SerializeField]
    private Condition[] conditions;
    public string Id => id;
    public LocationDefinition Destination => destination;

    public bool IsAvailable(GameStateStore state) =>
        destination != null && ConditionResolver.All(conditions, state);
}
