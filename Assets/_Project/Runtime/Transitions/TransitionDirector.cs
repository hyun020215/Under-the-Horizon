using System.Threading.Tasks;
using UnityEngine;

public sealed class TransitionDirector : MonoBehaviour
{
    [SerializeField]
    private TransitionPlayer[] players;

    [SerializeField]
    private UIInputBlocker blocker;

    public Task BeginAsync(TransitionProfile profile) => Play(profile, true);

    public Task EndAsync(TransitionProfile profile) => Play(profile, false);

    private async Task Play(TransitionProfile profile, bool entering)
    {
        if (profile == null)
            return;
        if (blocker != null && profile.blockInput)
            blocker.SetBlocked(true);
        if (profile.stinger != null)
            AudioSource.PlayClipAtPoint(profile.stinger, Vector3.zero);
        if (players != null)
            foreach (var player in players)
                if (player != null && player.Supports(profile.type))
                {
                    await player.PlayAsync(new TransitionRequest(profile, entering));
                    break;
                }
        if (!entering && blocker != null)
            blocker.SetBlocked(false);
    }
}
