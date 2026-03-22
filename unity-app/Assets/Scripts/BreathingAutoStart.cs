using UnityEngine;

// Small helper: when placed in the BreathingExercise scene, this will find the
// BreathingController in the scene and call StartBreathing() automatically.
public class BreathingAutoStart : MonoBehaviour
{
    [Tooltip("Optional delay (seconds) before starting the breathing loop")]
    public float startDelay = 0.1f;

    void Start()
    {
        if (startDelay <= 0f)
        {
            StartNow();
        }
        else
        {
            Invoke(nameof(StartNow), startDelay);
        }
    }

    void StartNow()
    {
        var ctrl = FindObjectOfType<BreathingController>();
        if (ctrl != null)
        {
            Debug.Log("BreathingAutoStart: found BreathingController, starting breathing.");
            ctrl.StartBreathing();
        }
        else
        {
            Debug.LogWarning("BreathingAutoStart: no BreathingController found in scene.");
        }
    }
}
