using System.Threading.Tasks;
using UnityEngine;

public abstract class InteractionAction : ScriptableObject
{
    public virtual bool GrantsEvidence => false;

    public abstract Task<InteractionResult> ExecuteAsync(InteractionContext context);
}
