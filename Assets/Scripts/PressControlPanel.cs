using TMPro;
using UnityEngine;

public class PressControlPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PressController controller;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text feedbackText;

    private void OnEnable()
    {
        if (controller == null || statusText == null || feedbackText == null)
        {
            Debug.LogError("UI-Referenzen fehlen!", this);
            return;
        }

        controller.StateChanged += UpdateStatus;
        controller.FeedbackChanged += UpdateFeedback;

        UpdateStatus(controller.CurrentState);
        UpdateFeedback(controller.CurrentFeedback);
    }

    private void OnDisable()
    {
        if (controller != null)
        {
            controller.StateChanged -= UpdateStatus;
            controller.FeedbackChanged -= UpdateFeedback;
        }
    }

    private void UpdateStatus(PressState state)
    {
        switch (state)
        {
            case PressState.Off:
                statusText.text = "OFF";
                statusText.color = new Color32(248, 113, 113, 255);
                break;

            case PressState.Idle:
                statusText.text = "IDLE";
                statusText.color = new Color32(74, 222, 128, 255);
                break;

            case PressState.Pressing:
                statusText.text = "PRESSING";
                statusText.color = new Color32(250, 204, 21, 255);
                break;

            case PressState.EmergencyStopped:
                statusText.text = "EMERGENCY STOP";
                statusText.color = new Color32(239, 68, 68, 255);
                break;
        }
    }

    private void UpdateFeedback(string message)
    {
        feedbackText.text = message;
    }
}