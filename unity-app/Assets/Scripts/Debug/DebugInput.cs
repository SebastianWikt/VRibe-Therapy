// using UnityEngine;
// using UnityEngine.InputSystem;

// public class DebugInput : MonoBehaviour
// {
//     public RegulationStateManager regulationStateManager;
//     public float changeAmount = 0.2f;

//     private void Update()
//     {
//         if (regulationStateManager == null || Keyboard.current == null)
//             return;

//         if (Keyboard.current.jKey.wasPressedThisFrame)
//         {
//             regulationStateManager.rawCalmScore =
//                 Mathf.Clamp01(regulationStateManager.rawCalmScore + changeAmount);

//             Debug.Log("J pressed, rawCalmScore = " + regulationStateManager.rawCalmScore);
//         }

//         if (Keyboard.current.kKey.wasPressedThisFrame)
//         {
//             regulationStateManager.rawCalmScore =
//                 Mathf.Clamp01(regulationStateManager.rawCalmScore - changeAmount);

//             Debug.Log("K pressed, rawCalmScore = " + regulationStateManager.rawCalmScore);
//         }
//     }
// }

using UnityEngine;
using UnityEngine.InputSystem;
using LSL;
using System.Collections;

public class VibeBridge : MonoBehaviour
{
    [Header("Dependencies")]
    public RegulationStateManager regulationStateManager;
    public string streamName = "VibeStream";

    [Header("Manual Debug Settings")]
    public float changeAmount = 0.1f;

    private StreamInlet inlet;
    private float[] sample;

    void Start()
    {
        if (regulationStateManager == null)
            Debug.LogError("RegulationStateManager is missing! Drag it into the Inspector.");

        StartCoroutine(ResolveLSLStream());
    }

    IEnumerator ResolveLSLStream()
    {
        Debug.Log($">>> Searching for LSL Stream: {streamName} <<<");
        StreamInfo[] results = LSL.LSL.resolve_stream("name", streamName, 1, 2.0);
        
        if (results.Length > 0)
        {
            inlet = new StreamInlet(results[0]);
            sample = new float[results[0].channel_count()];
            Debug.Log($"SUCCESS: Connected to {streamName}!");
        }
        else
        {
            Debug.LogWarning($"LSL Stream '{streamName}' not found. Defaulting to Keyboard Debug Mode.");
        }
        yield return null;
    }

    void Update()
    {
        if (regulationStateManager == null) return;

        if (inlet == null && Time.frameCount % 300 == 0) 
        {
            StartCoroutine(ResolveLSLStream());
            return;
        }

        // --- 1. HANDLE REAL EEG DATA (LSL) ---
        if (inlet != null)
        {
            Debug.Log($"in LSL");
            double timestamp = inlet.pull_sample(sample, 0.0f);
            while (inlet.pull_sample(sample, 0.0f) != 0) 
            {
                // Now 'sample' actually contains your Alpha/Beta!
                float alpha = sample[0];
                float beta = sample[1];
                
                float rawRatio = alpha / (alpha + beta + 0.001f);

                // 2. Define your observed range (Tweak these if needed)
                float minObserved = 0.35f; 
                float maxObserved = 0.75f;

                // 3. Map the rawRatio into a 0 to 1 range
                // This stretches the 0.35-0.75 jump into a full 0.0-1.0 jump
                float brainScore = (rawRatio - minObserved) / (maxObserved - minObserved);
                regulationStateManager.rawCalmScore = Mathf.Clamp01(brainScore);
                
                Debug.Log($"SUCCESS: New Data Pulled! Score: {brainScore}");
                Debug.Log($"Raw: {rawRatio:F2} | Scaled Score: {brainScore:F2}");
            }
        }

        // --- 2. HANDLE KEYBOARD OVERRIDE (J/K) ---
        // if (Keyboard.current != null)
        // {
        //     Debug.Log($"in keyboard");
        //     if (Keyboard.current.jKey.wasPressedThisFrame)
        //     {
        //         regulationStateManager.rawCalmScore = Mathf.Clamp01(regulationStateManager.rawCalmScore + changeAmount);
        //         Debug.Log("MANUAL OVERRIDE: CalmScore + -> " + regulationStateManager.rawCalmScore);
        //     }

        //     if (Keyboard.current.kKey.wasPressedThisFrame)
        //     {
        //         regulationStateManager.rawCalmScore = Mathf.Clamp01(regulationStateManager.rawCalmScore - changeAmount);
        //         Debug.Log("MANUAL OVERRIDE: CalmScore - -> " + regulationStateManager.rawCalmScore);
        //     }
        // }
    }
}