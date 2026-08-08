using UnityEngine;
using UnityEngine.UI;

public sealed class UiFeedbackInstaller : MonoBehaviour
{
    [SerializeField]
    private SfxController sfx;

    [SerializeField]
    private AudioClip hoverClip;

    [SerializeField]
    private AudioClip clickClip;

    private void Awake()
    {
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            UiButtonFeedback feedback = button.GetComponent<UiButtonFeedback>();
            if (feedback == null)
                feedback = button.gameObject.AddComponent<UiButtonFeedback>();
            feedback.Configure(sfx, hoverClip, clickClip);
        }
    }
}
