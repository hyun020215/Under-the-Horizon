using System.Threading.Tasks;
using UnityEngine;

public sealed class TransitionDirector : MonoBehaviour
{
    [SerializeField]
    private TransitionPlayer[] players;

    [SerializeField]
    private UIInputBlocker blocker;

    [SerializeField]
    private SfxController sfx;
    private AccessibilitySettingsService accessibility;

    private void Awake() => AppContext.Services?.TryGet(out accessibility);

    public Task BeginAsync(TransitionProfile profile) => Play(profile, true);

    public Task EndAsync(TransitionProfile profile) => Play(profile, false);

    private async Task Play(TransitionProfile profile, bool entering)
    {
        if (profile == null)
            return;
        if (accessibility == null)
            AppContext.Services?.TryGet(out accessibility);
        if (accessibility?.ReducedMotion == true)
        {
            if (blocker != null)
                blocker.SetBlocked(false);
            return;
        }
        if (blocker != null && profile.blockInput)
            blocker.SetBlocked(true);
        if (entering && profile.stinger != null)
            sfx?.Play(profile.stinger);
        if (entering)
            await WaitAsync(profile.uiExitDuration);
        if (players != null)
            foreach (var player in players)
                if (player != null && player.Supports(profile.type))
                {
                    await player.PlayAsync(new TransitionRequest(profile, entering));
                    break;
                }
        if (entering)
            await WaitAsync(profile.holdDuration);
        else
            await WaitAsync(profile.uiEnterDuration);
        if (!entering && blocker != null)
            blocker.SetBlocked(false);
    }

    private static async Task WaitAsync(float duration)
    {
        if (duration <= 0f)
            return;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            await Task.Yield();
            elapsed += Time.unscaledDeltaTime;
        }
    }
}
