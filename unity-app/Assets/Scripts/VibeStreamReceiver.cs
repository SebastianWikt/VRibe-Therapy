using UnityEngine;
using LSL;
using System.Collections;

public class VibeStreamReceiver : MonoBehaviour
{
    public string StreamName = "VibeStream";
    private StreamInlet inlet;
    private float[] sample;

    // These are your "Pancaking" outputs for other scripts to use
    [Header("Current Vibe State")]
    public float alphaPower;
    public float betaPower;
    public float calmScore; // Normalized 0 to 1

    void Start()
    {
        StartCoroutine(ResolveStream());
    }

    IEnumerator ResolveStream()
    {
        // Look for the stream broadcast by your Python script
        StreamInfo[] results = LSL.LSL.resolve_stream("name", StreamName, 1, 2.0);
        
        if (results.Length > 0)
        {
            inlet = new StreamInlet(results[0]);
            sample = new float[results[0].channel_count()];
            Debug.Log($"Connected to {StreamName}!");
        }
        else
        {
            Debug.LogError($"Could not find LSL stream: {StreamName}. Is Python running?");
        }
        yield return null;
    }

    void Update()
    {
        if (inlet == null) return;

        // Pull the latest data from the network buffer
        double timestamp = inlet.pull_sample(sample, 0.0f);

        if (timestamp != 0)
        {
            // [0] = Alpha, [1] = Beta (matching our Python script)
            alphaPower = sample[0];
            betaPower = sample[1];

            // Calculate the calmScore (Simple ratio normalized)
            // Example logic: Alpha high + Beta low = 1.0 (Very Calm)
            float ratio = alphaPower / (betaPower + 0.001f); 
            calmScore = Mathf.Clamp01(ratio / 2.0f); // Adjust 2.0f based on your baseline
        }
    }
}