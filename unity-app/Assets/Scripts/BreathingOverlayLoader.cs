using System.Collections;
using UnityEngine;

// Parent-side helper: starts the breathing overlay, hides the cube that triggered
// it, and restores the cube when the overlay exits.
public class BreathingOverlayLoader : MonoBehaviour
{
    [Tooltip("Delay before cleanup runs after exit, to let XR subsystems quiesce")]
    public float unloadDelay = 0.25f;

    [Tooltip("Uncheck this — the BreathingController has its own 3D hand-trackable exit button")]
    public bool createParentExitButton = false;

    [Header("Breathing Controller")]
    [Tooltip("Drag the BreathingController scene object (under CenterEyeAnchor) here — NOT a prefab asset")]
    public BreathingController breathingController;

    private BreathingController controller;
    private GameObject hiddenObject;
    private bool hiddenObjectWasActive = false;

    // -------------------------------------------------------------------------
    // Public API — called by BreathBlockBehavior / CubePopStarter
    // -------------------------------------------------------------------------

    public void StartBreathingExercise()
    {
        hiddenObject = null;
        hiddenObjectWasActive = false;
        StartCoroutine(LoadAndStart());
    }

    public void StartBreathingExerciseFor(GameObject hideTarget)
    {
        hiddenObject = hideTarget;
        hiddenObjectWasActive = hideTarget != null && hideTarget.activeSelf;
        StartCoroutine(LoadAndStart());
    }

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------

    private IEnumerator LoadAndStart()
    {
        if (breathingController == null)
        {
            Debug.LogWarning("BreathingOverlayLoader: no BreathingController assigned. " +
                             "Drag the scene instance (not a prefab) into the Inspector field.");
            yield break;
        }

        controller = breathingController;
        controller.destroyOnExit = false;

        // Subscribe to exit event so we can restore the cube and clean up
        controller.onExit.AddListener(OnOverlayExit);

        // Hide the cube while breathing is active
        if (hiddenObject != null)
        {
            hiddenObjectWasActive = hiddenObject.activeSelf;
            hiddenObject.SetActive(false);
        }

        // Start the breathing loop
        controller.StartBreathing();
        Debug.Log("BreathingOverlayLoader: breathing started.");

        yield break;
    }

    private void OnOverlayExit()
    {
        StartCoroutine(UnloadOverlaySafe());
    }

    private IEnumerator UnloadOverlaySafe()
    {
        // Brief pause so any XR state settles before we restore the scene
        yield return new WaitForSeconds(unloadDelay);

        // Restore the hidden object (the cube)
        if (hiddenObject != null)
        {
            hiddenObject.SetActive(hiddenObjectWasActive);
            hiddenObject = null;
        }

        // Unsubscribe so the listener doesn't fire again next session
        if (controller != null)
        {
            controller.onExit.RemoveListener(OnOverlayExit);
            controller = null;
        }

        Debug.Log("BreathingOverlayLoader: cleanup complete, cube restored.");
    }
}