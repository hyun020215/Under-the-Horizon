using System.Threading.Tasks;using UnityEngine; public abstract class InteractionAction:ScriptableObject { public abstract Task<InteractionResult> ExecuteAsync(InteractionContext context); }
