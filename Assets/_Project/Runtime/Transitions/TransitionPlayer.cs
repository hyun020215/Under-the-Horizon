using System.Threading.Tasks;
using UnityEngine;

public abstract class TransitionPlayer : MonoBehaviour
{
    public abstract bool Supports(TransitionType type);
    public abstract Task PlayAsync(TransitionRequest request);
}
