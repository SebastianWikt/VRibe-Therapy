using UnityEngine;

public class RegulationStateManager : MonoBehaviour
{
    public RegulationState currentState = RegulationState.Unknown;

    [Header("Signal")]
    [Range(0f, 1f)] public float rawCalmScore = 0.5f;
    [Range(0f, 1f)] public float smoothedCalmScore = 0.5f;
    public float smoothingSpeed = 2f;

    [Header("Thresholds")]
    public float unsettledThreshold = 0.35f;
    public float settlingThreshold = 0.55f;

    public float calmEnterThreshold = 0.72f;
    public float calmExitThreshold = 0.62f;

    public float focusedEnterThreshold = 0.85f;
    public float focusedExitThreshold = 0.78f;

    [Header("Timing")]
    public float timeToLeaveUnknown = 2f;
    public float timeToEnterSettling = 1.5f;
    public float timeToEnterCalm = 3f;
    public float timeToExitCalm = 2f;
    public float timeToEnterFocusedCalm = 4f;
    public float timeToExitFocusedCalm = 2f;

    private float unknownTimer;
    private float settlingTimer;
    private float calmEnterTimer;
    private float calmExitTimer;
    private float focusedEnterTimer;
    private float focusedExitTimer;

    private void Update()
    {
        rawCalmScore = Mathf.Clamp01(rawCalmScore);

        SmoothSignal();
        EvaluateState();

        // Debug.Log("Raw Calm: " + rawCalmScore +
        //           " | Smoothed Calm: " + smoothedCalmScore +
        //           " | State: " + currentState);
    }

    private void SmoothSignal()
    {
        smoothedCalmScore = Mathf.Lerp(
            smoothedCalmScore,
            rawCalmScore,
            Mathf.Clamp01(smoothingSpeed * Time.deltaTime)
        );
    }

    private void EvaluateState()
    {
        switch (currentState)
        {
            case RegulationState.Unknown:
                HandleUnknown();
                break;

            case RegulationState.Unsettled:
                HandleUnsettled();
                break;

            case RegulationState.Settling:
                HandleSettling();
                break;

            case RegulationState.Calm:
                HandleCalm();
                break;

            case RegulationState.FocusedCalm:
                HandleFocusedCalm();
                break;
        }
    }

    private void HandleUnknown()
    {
        if (smoothedCalmScore > 0.1f)
        {
            unknownTimer += Time.deltaTime;
            if (unknownTimer >= timeToLeaveUnknown)
            {
                SetState(RegulationState.Unsettled);
            }
        }
        else
        {
            unknownTimer = 0f;
        }
    }

    private void HandleUnsettled()
    {
        if (smoothedCalmScore >= settlingThreshold)
        {
            settlingTimer += Time.deltaTime;
            if (settlingTimer >= timeToEnterSettling)
            {
                SetState(RegulationState.Settling);
            }
        }
        else
        {
            settlingTimer = 0f;
        }
    }

    private void HandleSettling()
    {
        if (smoothedCalmScore >= calmEnterThreshold)
        {
            calmEnterTimer += Time.deltaTime;
            if (calmEnterTimer >= timeToEnterCalm)
            {
                SetState(RegulationState.Calm);
            }
        }
        else
        {
            calmEnterTimer = 0f;
        }

        if (smoothedCalmScore < unsettledThreshold)
        {
            SetState(RegulationState.Unsettled);
        }
    }

    private void HandleCalm()
    {
        if (smoothedCalmScore < calmExitThreshold)
        {
            calmExitTimer += Time.deltaTime;
            if (calmExitTimer >= timeToExitCalm)
            {
                SetState(RegulationState.Settling);
            }
        }
        else
        {
            calmExitTimer = 0f;
        }

        if (smoothedCalmScore >= focusedEnterThreshold)
        {
            focusedEnterTimer += Time.deltaTime;
            if (focusedEnterTimer >= timeToEnterFocusedCalm)
            {
                SetState(RegulationState.FocusedCalm);
            }
        }
        else
        {
            focusedEnterTimer = 0f;
        }
    }

    private void HandleFocusedCalm()
    {
        if (smoothedCalmScore < focusedExitThreshold)
        {
            focusedExitTimer += Time.deltaTime;
            if (focusedExitTimer >= timeToExitFocusedCalm)
            {
                SetState(RegulationState.Calm);
            }
        }
        else
        {
            focusedExitTimer = 0f;
        }
    }

    private void SetState(RegulationState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        ResetAllTimers();

        Debug.Log("Regulation state changed to: " + currentState);
    }

    private void ResetAllTimers()
    {
        unknownTimer = 0f;
        settlingTimer = 0f;
        calmEnterTimer = 0f;
        calmExitTimer = 0f;
        focusedEnterTimer = 0f;
        focusedExitTimer = 0f;
    }
}