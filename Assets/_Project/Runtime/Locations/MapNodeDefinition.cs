using System;
using UnityEngine;

public enum MapNodeAccessMode
{
    PersistentUnlock = 0,
    RouteOnly = 1,
}

[Serializable]
public sealed class MapNodeDefinition
{
    [SerializeField]
    private string id;

    [SerializeField]
    private Vector2 normalizedPosition;

    [SerializeField]
    private string displayName;

    [SerializeField, TextArea]
    private string description;

    [SerializeField]
    private MapNodeAccessMode accessMode;

    public string Id => id;
    public Vector2 NormalizedPosition => normalizedPosition;
    public string DisplayName => displayName;
    public string Description => description;
    public MapNodeAccessMode AccessMode => accessMode;
}
