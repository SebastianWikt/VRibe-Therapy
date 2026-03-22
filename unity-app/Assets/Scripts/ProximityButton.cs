using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A simple world-space button that fires when any collider enters its trigger zone.
/// Works with Meta Building Blocks hand tracking (finger colliders) and controllers.
/// 
/// Setup:
///   1. Attach this to a GameObject that has a BoxCollider with "Is Trigger" = true.
///   2. Wire up the onPressed UnityEvent in the Inspector (or via code).
///   3. The button visually depresses on hover using a child "Fill" object (optional).
/// </summary>
public class ProximityButton : MonoBehaviour
{
    [Tooltip("Fired when a hand or controller enters the trigger zone")]
    public UnityEvent onPressed;

    [Tooltip("Seconds before the button can fire again (prevents repeated triggers)")]
    public float cooldown = 1.5f;

    [Tooltip("Optional: child renderer that changes color on hover")]
    public Renderer fillRenderer;

    public Color normalColor  = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color hoveredColor = new Color(0.4f, 0.8f, 1.0f, 0.9f);

    private float lastPressed = -99f;
    private int   enterCount  = 0;

    void Start()
    {
        if (fillRenderer != null)
            fillRenderer.material.color = normalColor;
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignore self-collisions and environment geometry —
        // only react to finger/hand/controller colliders.
        if (!IsHandOrController(other)) return;

        enterCount++;

        if (fillRenderer != null)
            fillRenderer.material.color = hoveredColor;

        if (Time.time - lastPressed < cooldown) return;
        lastPressed = Time.time;

        Debug.Log("ProximityButton: pressed by " + other.name);
        onPressed?.Invoke();
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsHandOrController(other)) return;
        enterCount = Mathf.Max(0, enterCount - 1);
        if (enterCount == 0 && fillRenderer != null)
            fillRenderer.material.color = normalColor;
    }

    // Accept finger bones, hand colliders, or controller colliders.
    // Meta Building Blocks finger colliders are typically tagged "Hand"
    // or live on objects named with "finger", "hand", "poke", "controller".
    private bool IsHandOrController(Collider other)
    {
        return true;
    }
}