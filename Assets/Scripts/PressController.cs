using System;
using UnityEngine;

public class PressController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PressView pressView;

    private PressMachine machine;
    public event Action<PressState> StateChanged;
    public event Action<string> FeedbackChanged;

    public string CurrentFeedback { get; private set; } = "Machine powered off.";

    public PressState CurrentState => machine?.State ?? PressState.Off;

    private void Awake()
    {
        if (pressView == null)
        {
            Debug.LogError("PressView fehlt!", this);
            enabled = false;
            return;
        }

        machine = new PressMachine();

        // Unsere Schutzabdeckung startet geschlossen.
        machine.SetCoverClosed(true);
    }

    private void OnEnable()
    {
        if (machine == null || pressView == null)
            return;

        machine.StateChanged += OnStateChanged;
        pressView.CycleCompleted += OnCycleCompleted;
        pressView.CoverMovementCompleted += OnCoverMovementCompleted;
        pressView.ReturnHomeCompleted += OnReturnHomeCompleted;
    }

    private void OnDisable()
    {
        if (machine != null)
            machine.StateChanged -= OnStateChanged;

        if (pressView != null)
        {
            pressView.CycleCompleted -= OnCycleCompleted;
            pressView.CoverMovementCompleted -= OnCoverMovementCompleted;
            pressView.ReturnHomeCompleted -= OnReturnHomeCompleted;
        }
    }

    private void OnReturnHomeCompleted()
    {
        SetFeedback("Ram returned to home position.");
    }

    public void PowerOn()
    {
        if (machine.State != PressState.Off)
        {
            SetFeedback("Machine is already active.");
            return;
        }

        machine.PowerOn();
        SetFeedback("Machine ready.");
    }


    public void SetWorkpiecePresent(bool present)
    {
        if (machine.State == PressState.Pressing ||
            machine.State == PressState.EmergencyStopped)
        {
            SetFeedback("Workpiece cannot be changed right now.");
            return;
        }

        if (!pressView.IsCoverOpen)
        {
            SetFeedback("Open safety cover first.");
            return;
        }

        machine.SetWorkpiecePresent(present);
        pressView.SetWorkpieceVisible(machine.HasWorkpiece);

        SetFeedback(
            present
                ? "Workpiece inserted."
                : "Workpiece removed."
        );
    }


    // Schutzabdeckung öffnen
    public void RequestCoverOpen()
    {
        if (machine.State == PressState.Pressing)
        {
            SetFeedback("Cover locked during press cycle.");
            return;
        }

        machine.SetCoverClosed(false);
        SetFeedback("Opening safety cover...");

        pressView.MoveCover(true);
    }

    // Schutzabdeckung schließen
    public void RequestCoverClose()
    {
        if (machine.State == PressState.Pressing)
        {
            SetFeedback("Cover locked during press cycle.");
            return;
        }

        machine.SetCoverClosed(false);
        SetFeedback("Closing safety cover...");

        pressView.MoveCover(false);
    }

    // Wird nach Abschluss der Türanimation aufgerufen.
    private void OnCoverMovementCompleted(bool isClosed)
    {
        machine.SetCoverClosed(isClosed);

        SetFeedback(
            isClosed
                ? "Safety cover closed."
                : "Safety cover open."
        );
    }

    public void RequestStart()
    {
        if (!pressView.IsAtHome)
        {
            SetFeedback("Return ram to home position first.");
            return;
        }

        if (machine.TryStartPress())
        {
            SetFeedback("Press cycle running...");
            pressView.StartCycle();
            return;
        }

        if (machine.State == PressState.Off)
        {
            SetFeedback("Power on the machine first.");
        }
        else if (machine.State == PressState.EmergencyStopped)
        {
            SetFeedback("Reset emergency stop first.");
        }
        else if (!machine.IsCoverClosed)
        {
            SetFeedback("Close safety cover first.");
        }
        else if (!machine.HasWorkpiece)
        {
            SetFeedback("Insert a workpiece first.");
        }
        else
        {
            SetFeedback("Start currently not allowed.");
        }
    }

    private void OnCycleCompleted()
    {
        machine.CompletePress();
        SetFeedback("Press cycle completed.");
    }

    private void OnStateChanged(PressState state)
    {
        Debug.Log($"Maschinenzustand: {state}", this);

        if (state == PressState.EmergencyStopped)
        {
            pressView.StopCycle();
        }

        StateChanged?.Invoke(state);
    }

    // TESTFUNKTIONEN

    [ContextMenu("Test / Vorbereitung")]
    private void TestPreparation()
    {
        if (!Application.isPlaying)
            return;

        PowerOn();
        SetWorkpiecePresent(true);
    }

    [ContextMenu("Test / Abdeckung oeffnen")]
    private void TestOpenCover()
    {
        if (Application.isPlaying)
            RequestCoverOpen();
    }

    [ContextMenu("Test / Abdeckung schliessen")]
    private void TestCloseCover()
    {
        if (Application.isPlaying)
            RequestCoverClose();
    }

    [ContextMenu("Test / Start anfordern")]
    private void TestStart()
    {
        if (Application.isPlaying)
            RequestStart();
    }

    public void EmergencyStop()
    {
        machine.EmergencyStop();
        SetFeedback("Emergency stop activated.");
    }
    
    [ContextMenu("Test / Not-Halt")]
    private void TestEmergencyStop()
    {
        if (Application.isPlaying)
            EmergencyStop();
    }

    [ContextMenu("Test / Not-Halt zuruecksetzen")]
    private void TestResetEmergencyStop()
    {
        if (Application.isPlaying)
            ResetEmergencyStop();
    }

    public void ResetEmergencyStop()
    {
        if (machine.State != PressState.EmergencyStopped)
        {
            SetFeedback("No emergency stop to reset.");
            return;
        }

        machine.ResetEmergencyStop();
        SetFeedback("Emergency stop reset. Return ram home.");
    }

    public void RequestReturnHome()
    {
        if (machine.State == PressState.EmergencyStopped)
        {
            SetFeedback("Reset emergency stop first.");
            return;
        }

        if (machine.State != PressState.Off)
        {
            SetFeedback("Machine must be OFF before returning ram.");
            return;
        }

        if (pressView.IsAtHome)
        {
            SetFeedback("Ram is already in home position.");
            return;
        }

        SetFeedback("Returning ram to home position...");
        pressView.ReturnToHome();
    }

    private void SetFeedback(string message)
    {
        CurrentFeedback = message;
        FeedbackChanged?.Invoke(message);
    }

    [ContextMenu("Test / Stempel zurueckfahren")]
    private void TestReturnHome()
    {
        if (Application.isPlaying)
            RequestReturnHome();
    }
}
