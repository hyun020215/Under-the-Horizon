using UnityEngine;

[CreateAssetMenu(fileName = "DATABASE_", menuName = "Under The Horizon/Content/Catalog")]
public sealed class ContentCatalog : ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private Object[] entries;

    public string Id => id;
    public Object[] Entries => entries;
}
