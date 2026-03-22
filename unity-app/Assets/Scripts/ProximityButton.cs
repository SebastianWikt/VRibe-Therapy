using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// World-space button that fires when any collider enters its trigger zone.
/// Works with Meta Building Blocks hand tracking and controllers.
/// Attach to a GameObject with a BoxCollider (Is Trigger = true).
/// </summary>
public class ProximityButton : MonoBehaviour
{
    [Tooltip("Fired when anything enters the trigger zone")]
    public UnityEvent onPressed;

    [Tooltip("Seconds before the button can fire again")]
    public float cooldown = 1.5f;

    [Tooltip("Optional renderer that changes color on hover")]
    public Renderer fillRenderer;

    public Color normalColor  = new Color(0.08f, 0.08f, 0.14f, 1f);
    public Color hoveredColor = new Color(0.25f, 0.65f, 1.0f,  1f);

    private float lastPressed = -99f;
    private int   enterCount  = 0;

    void Start()
    {
        if (fillRenderer != null)
            fillRenderer.material.color = normalColor;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)    return;
        if (IsEnvironment(other)) return;

        enterCount++;
        if (fillRenderer != null)
            fillRenderer.material.color = hoveredColor;

        if (Time.time - lastPressed < cooldown) return;
        lastPressed = Time.time;

        Debug.Log("ProximityButton pressed by: " + other.gameObject.name);
        onPressed?.Invoke();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)    return;
        if (IsEnvironment(other)) return;

        enterCount = Mathf.Max(0, enterCount - 1);
        if (enterCount == 0 && fillRenderer != null)
            fillRenderer.material.color = normalColor;
    }

    // Filter out large static environment colliders (floors, walls, scene geometry).
    // Everything else — hands, fingers, controllers — is welcome.
    private bool IsEnvironment(Collider other)
    {
        string layerName = LayerMask.LayerToName(other.gameObject.layer).ToLower();
        if (layerName.Contains("ground")
         || layerName.Contains("terrain")
         || layerName.Contains("environment")
         || layerName.Contains("world")
         || layerName.Contains("static"))
            return true;

        // Skip very large colliders (likely floors/walls, not hands)
        Vector3 s = other.bounds.size;
        if (s.x > 2f || s.y > 2f || s.z > 2f)
            return true;

        return false;
    }
}