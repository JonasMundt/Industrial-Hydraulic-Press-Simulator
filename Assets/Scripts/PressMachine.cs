
using System;

public class PressMachine
{
    // Aktueller Maschinenzustand
    public PressState State { get; private set; }
        = PressState.Off;

    // Voraussetzungen für den Pressvorgang
    public bool IsCoverClosed { get; private set; }
    public bool HasWorkpiece { get; private set; }

    // Benachrichtigung bei Zustandsänderungen
    public event Action<PressState> StateChanged;

    // Maschine einschalten
    public void PowerOn()
    {
        if (State != PressState.Off)
            return;

        ChangeState(PressState.Idle);
    }

    // Schutzabdeckung und Werkstück
    public void SetCoverClosed(bool closed)
    {
        if (State == PressState.Pressing)
            return;

        IsCoverClosed = closed;
    }

    public void SetWorkpiecePresent(bool present)
    {
        if (State == PressState.Pressing)
            return;

        HasWorkpiece = present;
    }

    // Pressvorgang anfordern
    public bool TryStartPress()
    {
        if (State != PressState.Idle)
            return false;

        if (!IsCoverClosed || !HasWorkpiece)
            return false;

        ChangeState(PressState.Pressing);
        return true;
    }

    // Pressvorgang erfolgreich abgeschlossen
    public void CompletePress()
    {
        if (State != PressState.Pressing)
            return;

        ChangeState(PressState.Idle);
    }

    // Not-Halt auslösen
    public void EmergencyStop()
    {
        if (State == PressState.EmergencyStopped)
            return;

        ChangeState(PressState.EmergencyStopped);
    }

    // Not-Halt zurücksetzen
    public void ResetEmergencyStop()
    {
        if (State != PressState.EmergencyStopped)
            return;

        ChangeState(PressState.Off);
    }

    // Zentraler Zustandswechsel
    private void ChangeState(PressState newState)
    {
        State = newState;
        StateChanged?.Invoke(State);
    }
}
