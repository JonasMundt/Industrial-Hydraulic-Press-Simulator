
using System;
using System.Collections;
using UnityEngine;

public class PressView : MonoBehaviour
{
    [Header("Press Components")]
    [SerializeField] private Transform pressRam;

    [Header("Workpiece")]
    [SerializeField] private GameObject workpiece;

    [Header("Animation Settings")]
    [SerializeField] private float travelDistance = 0.15f;
    [SerializeField] private float movementDuration = 1f;

    // Benachrichtigt andere Klassen nach einem Presszyklus.
    public event Action CycleCompleted;
    public event Action ReturnHomeCompleted;

    private Vector3 startPosition;
    private Coroutine runningCycle;

    [Header("Safety Cover")]
    [SerializeField] private Transform safetyCover;
    [SerializeField] private float coverTravelDistance = 0.6f;
    [SerializeField] private float coverMovementDuration = 1f;

    private Coroutine recoveryRoutine;

    public bool IsCoverOpen =>
    safetyCover != null &&
    coverRoutine == null &&
    Vector3.Distance(
        safetyCover.localPosition,
        coverClosedPosition + Vector3.up * coverTravelDistance
    ) < 0.001f;

    public bool IsAtHome =>
        pressRam != null &&
        recoveryRoutine == null &&
        Vector3.Distance(
            pressRam.localPosition,
            startPosition
        ) < 0.001f;

    private Vector3 coverClosedPosition;
    private Coroutine coverRoutine;

    // true bedeutet: Schutzabdeckung ist vollständig geschlossen.
    public event Action<bool> CoverMovementCompleted;

    private void Awake()
    {
        if (pressRam == null)
        {
            Debug.LogError("Press_Ram fehlt!", this);
            enabled = false;
            return;
        }

        startPosition = pressRam.localPosition;
        if (safetyCover != null)
        {
            coverClosedPosition = safetyCover.localPosition;
        }

        if (workpiece != null)
        {
            workpiece.SetActive(false);
        }
    }

    // Wird später vom PressController aufgerufen.
    public void StartCycle()
    {
        if (!isActiveAndEnabled ||
        runningCycle != null ||
        !IsAtHome)
        {
            return;
        }

        runningCycle = StartCoroutine(PressCycle());
    }

    // Unterbricht die Animation, z. B. beim Not-Halt.
    public void StopCycle()
    {
        if (runningCycle != null)
        {
            StopCoroutine(runningCycle);
            runningCycle = null;
        }

        if (recoveryRoutine != null)
        {
            StopCoroutine(recoveryRoutine);
            recoveryRoutine = null;
        }
    }

    private IEnumerator PressCycle()
    {
        Vector3 bottomPosition =
            startPosition + Vector3.down * travelDistance;

        // Herunterfahren
        yield return MoveRam(bottomPosition);

        // Hochfahren
        yield return MoveRam(startPosition);

        runningCycle = null;

        // Maschinensteuerung über Abschluss informieren.
        CycleCompleted?.Invoke();
    }

    private IEnumerator MoveRam(Vector3 targetPosition)
    {
        Vector3 fromPosition = pressRam.localPosition;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, movementDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            pressRam.localPosition = Vector3.Lerp(
                fromPosition,
                targetPosition,
                t
            );

            yield return null;
        }

        pressRam.localPosition = targetPosition;
    }

    public void MoveCover(bool open)
    {
        if (safetyCover == null || !isActiveAndEnabled)
            return;

        // Keine Türbewegung während eines Presszyklus.
        if (runningCycle != null)
            return;

        if (coverRoutine != null)
            StopCoroutine(coverRoutine);

        coverRoutine = StartCoroutine(AnimateCover(open));
    }

    private IEnumerator AnimateCover(bool open)
    {
        Vector3 targetPosition = coverClosedPosition;

        if (open)
            targetPosition += Vector3.up * coverTravelDistance;

        Vector3 fromPosition = safetyCover.localPosition;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, coverMovementDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            safetyCover.localPosition = Vector3.Lerp(
                fromPosition,
                targetPosition,
                t
            );

            yield return null;
        }

        safetyCover.localPosition = targetPosition;
        coverRoutine = null;

        CoverMovementCompleted?.Invoke(!open);
    }

    [ContextMenu("Test / Schutzabdeckung oeffnen")]
    private void TestOpenCover()
    {
        if (Application.isPlaying)
            MoveCover(true);
    }

    [ContextMenu("Test / Schutzabdeckung schliessen")]
    private void TestCloseCover()
    {
        if (Application.isPlaying)
            MoveCover(false);
    }

    public void ReturnToHome()
    {
        if (!isActiveAndEnabled ||
            runningCycle != null ||
            recoveryRoutine != null ||
            IsAtHome)
        {
            return;
        }

        recoveryRoutine = StartCoroutine(ReturnHomeRoutine());
    }

    private IEnumerator ReturnHomeRoutine()
    {
        yield return MoveRam(startPosition);

        recoveryRoutine = null;

        ReturnHomeCompleted?.Invoke();
    }

    public void SetWorkpieceVisible(bool visible)
    {
        if (workpiece == null)
        {
            Debug.LogError("Workpiece-Referenz fehlt!", this);
            return;
        }

        workpiece.SetActive(visible);
    }
}
