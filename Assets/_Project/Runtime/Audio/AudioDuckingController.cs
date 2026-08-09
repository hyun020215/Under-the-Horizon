using System.Collections;
using UnityEngine;

public sealed class AudioDuckingController : MonoBehaviour
{
    [SerializeField]
    private NarrativeDirector narrative;

    [SerializeField]
    private MusicController music;

    [SerializeField]
    private AmbienceController ambience;

    private DuckingProfile profile;
    private Coroutine transition;
    private float currentMusic = 1f;
    private float currentAmbience = 1f;

    public bool IsDucked { get; private set; }

    private void OnEnable()
    {
        if (narrative == null)
            return;
        narrative.DialogueStarted += BeginDucking;
        narrative.DialogueEnded += EndDucking;
    }

    private void OnDisable()
    {
        if (narrative == null)
            return;
        narrative.DialogueStarted -= BeginDucking;
        narrative.DialogueEnded -= EndDucking;
    }

    public void SetProfile(DuckingProfile value) => profile = value;

    public void BeginDucking()
    {
        IsDucked = true;
        StartTransition(
            profile != null ? profile.musicMultiplier : 0.45f,
            profile != null ? profile.ambienceMultiplier : 0.65f,
            profile != null ? profile.attack : 0.15f);
    }

    public void EndDucking()
    {
        IsDucked = false;
        StartTransition(
            1f,
            1f,
            profile != null ? profile.release : 0.3f);
    }

    private void StartTransition(float musicTarget, float ambienceTarget, float duration)
    {
        if (transition != null)
            StopCoroutine(transition);
        transition = StartCoroutine(
            TransitionTo(musicTarget, ambienceTarget, Mathf.Max(0f, duration)));
    }

    private IEnumerator TransitionTo(
        float musicTarget,
        float ambienceTarget,
        float duration)
    {
        float musicStart = currentMusic;
        float ambienceStart = currentAmbience;
        if (duration <= 0f)
        {
            Apply(musicTarget, ambienceTarget);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            Apply(
                Mathf.Lerp(musicStart, musicTarget, progress),
                Mathf.Lerp(ambienceStart, ambienceTarget, progress));
            yield return null;
        }

        Apply(musicTarget, ambienceTarget);
        transition = null;
    }

    private void Apply(float musicValue, float ambienceValue)
    {
        currentMusic = Mathf.Clamp01(musicValue);
        currentAmbience = Mathf.Clamp01(ambienceValue);
        music?.SetDuckingMultiplier(currentMusic);
        ambience?.SetDuckingMultiplier(currentAmbience);
    }
}
