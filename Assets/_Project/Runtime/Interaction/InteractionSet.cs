using UnityEngine;

[CreateAssetMenu(fileName = "INT_SET", menuName = "Under The Horizon/Interaction/Set")]
public sealed class InteractionSet : ScriptableObject
{
    [SerializeField]
    private InteractionDefinition[] interactions;
    public InteractionDefinition[] Interactions => interactions;
}
