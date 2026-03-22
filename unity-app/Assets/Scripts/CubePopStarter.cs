using UnityEngine;

// Simple helper to be attached to the cube (or the cube's spawner) so that
// when the cube is popped it will trigger the breathing overlay loader.
public class CubePopStarter : MonoBehaviour
{
    [Tooltip("Reference to the BreathingOverlayLoader in the parent scene. If empty, will attempt to FindObjectOfType at runtime.")]
    public BreathingOverlayLoader overlayLoader;

    // Call this method from your cube's pop event (e.g., UnityEvent, collider trigger, or other interaction handler)
    public void OnCubePopped()
    {
        if (overlayLoader == null)
        {
            overlayLoader = FindObjectOfType<BreathingOverlayLoader>();
        }

        if (overlayLoader != null)
        {
            // Prefer hiding the cube that invoked the overlay so it appears replaced
            try
            {
                overlayLoader.StartBreathingExerciseFor(this.gameObject);
            }
            catch
            {
                overlayLoader.StartBreathingExercise();
            }
        }
        else
        {
            Debug.LogWarning("CubePopStarter: No BreathingOverlayLoader found in scene. Assign one in the inspector or add a BreathingOverlayLoader to the parent scene.");
        }
    }
}
