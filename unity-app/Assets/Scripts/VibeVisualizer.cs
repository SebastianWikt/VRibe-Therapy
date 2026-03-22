using UnityEngine;

public class VibeVisualizer : MonoBehaviour
{
    [Range(0, 1)] public float calmScore = 0.5f; // Simulate this with the slider in Inspector
    public Color stressedColor = Color.red;
    public Color calmColor = Color.cyan;

    void Update()
    {
        // Change color based on "vibe"
        GetComponent<Renderer>().material.color = Color.Lerp(stressedColor, calmColor, calmScore);
        
        // Scale the object (Pulse effect)
        float pulse = 1.0f + (Mathf.Sin(Time.time * 2f) * 0.1f * (1 - calmScore));
        transform.localScale = Vector3.one * pulse;
    }
}