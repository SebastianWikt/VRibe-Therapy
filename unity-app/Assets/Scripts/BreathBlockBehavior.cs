using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Attach this to a grabbable block. Configure `breathingSceneName` to the scene to load.
public class BreathBlockBehavior : MonoBehaviour
{
    [Tooltip("Name of the breathing scene to load when the block is grabbed")]
    public string breathingSceneName = "BreathingExercise";

    [Tooltip("Scale multiplier for the pop effect when grabbed")]
    public float popScale = 1.35f;

    [Tooltip("Duration of the pop animation in seconds")]
    public float popDuration = 0.12f;

    [Header("Pop Visuals")]
    [Tooltip("Color to transition to while the block is being pulled")]
    public Color prePopColor = new Color(1f, 0.6f, 0.6f);

    [Tooltip("How long the color/scale transition before the final pop (seconds)")]
    public float prePopDuration = 0.25f;

    [Header("Two-Hand Pull Detection")]
    [Tooltip("If true, requires both controller anchors to be near the block and pulled apart to trigger")]
    public bool requireTwoHandPull = true;

    [Tooltip("Name of the left controller anchor GameObject to search for (case sensitive)")]
    public string leftAnchorName = "LeftControllerAnchor";

    [Tooltip("Name of the right controller anchor GameObject to search for (case sensitive)")]
    public string rightAnchorName = "RightControllerAnchor";

    [Tooltip("Maximum distance from the block the controller must be to count as 'grabbing' (meters)")]
    public float nearRadius = 0.18f;

    [Tooltip("How much the two-hand distance must increase (meters) to trigger the pop")]
    public float pullThreshold = 0.12f;

    Transform leftAnchor;
    Transform rightAnchor;
    bool trackingPull = false;
    float initialAnchorsDistance = 0f;
    bool triggered = false;

    // Called by grab UnityEvent (e.g. Grab/Select Entered)
    public void OnGrab()
    {
        if (!requireTwoHandPull)
        {
            StartCoroutine(PopAndLoad());
        }
        else
        {
            // if two-hand pull is required, start polling — actual trigger waits for pull
            TryBeginTrackingAnchors();
        }
    }

    // Optional: call this on release if you want to stop breathing or undo state
    public void OnRelease()
    {
        // if release happens before triggering, reset tracking
        StopTracking();
    }

    void Update()
    {
        if (!requireTwoHandPull || triggered) return;

        // If already tracking, check distance
        if (trackingPull && leftAnchor != null && rightAnchor != null)
        {
            float d = Vector3.Distance(leftAnchor.position, rightAnchor.position);
            if (d - initialAnchorsDistance > pullThreshold)
            {
                triggered = true;
                StartCoroutine(PopAndLoad());
            }
            // if anchors move away from block (no longer near), stop tracking
            if (!AreAnchorsNear())
            {
                StopTracking();
            }
        }
        else
        {
            // attempt to begin tracking if anchors are near
            if (leftAnchor == null || rightAnchor == null)
            {
                FindAnchorsIfMissing();
            }
            if (!trackingPull && AreAnchorsNear())
            {
                BeginTracking();
            }
        }
    }

    void TryBeginTrackingAnchors()
    {
        FindAnchorsIfMissing();
        if (leftAnchor != null && rightAnchor != null && AreAnchorsNear())
        {
            BeginTracking();
        }
    }

    void FindAnchorsIfMissing()
    {
        if (leftAnchor == null && !string.IsNullOrEmpty(leftAnchorName))
        {
            var go = GameObject.Find(leftAnchorName);
            if (go != null) leftAnchor = go.transform;
        }
        if (rightAnchor == null && !string.IsNullOrEmpty(rightAnchorName))
        {
            var go = GameObject.Find(rightAnchorName);
            if (go != null) rightAnchor = go.transform;
        }
    }

    bool AreAnchorsNear()
    {
        if (leftAnchor == null || rightAnchor == null) return false;
        float dl = Vector3.Distance(leftAnchor.position, transform.position);
        float dr = Vector3.Distance(rightAnchor.position, transform.position);
        return dl <= nearRadius && dr <= nearRadius;
    }

    void BeginTracking()
    {
        if (leftAnchor == null || rightAnchor == null) return;
        trackingPull = true;
        initialAnchorsDistance = Vector3.Distance(leftAnchor.position, rightAnchor.position);
    }

    void StopTracking()
    {
        trackingPull = false;
        initialAnchorsDistance = 0f;
    }

    IEnumerator PopAndLoad()
    {
        // Animated pre-pop: gradual grow + color change
        Vector3 original = transform.localScale;
        Vector3 preTarget = original * Mathf.Lerp(1.0f, popScale, 0.85f);
        Vector3 finalTarget = original * popScale;

        Renderer rend = GetComponent<Renderer>();
        Color originalColor = Color.white;
        if (rend != null) originalColor = rend.material.color;

        float t = 0f;
        float duration = Mathf.Max(prePopDuration, 0.01f);

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
            transform.localScale = Vector3.Lerp(original, preTarget, p);
            if (rend != null) rend.material.color = Color.Lerp(originalColor, prePopColor, p);
            yield return null;
        }

        // final quick pop (small overshoot then settle)
        float punchDur = Mathf.Max(popDuration, 0.04f);
        float half = punchDur * 0.5f;
        // overshoot
        float s = 0f;
        while (s < half)
        {
            s += Time.deltaTime;
            float p = Mathf.Clamp01(s / half);
            transform.localScale = Vector3.Lerp(preTarget, finalTarget * 1.08f, p);
            yield return null;
        }
        // settle back to finalTarget
        s = 0f;
        while (s < half)
        {
            s += Time.deltaTime;
            float p = Mathf.Clamp01(s / half);
            transform.localScale = Vector3.Lerp(finalTarget * 1.08f, finalTarget, p);
            yield return null;
        }

        // brief pause so the pop is perceptible
        yield return new WaitForSeconds(0.06f);

        // restore color to original on the object (optional)
        if (rend != null) rend.material.color = originalColor;

        // Prefer using a BreathingOverlayLoader if present.
        // First, check if a CubePopStarter on this object has an assigned loader (inspector-assigned).
        BreathingOverlayLoader loader = null;
        var starter = GetComponent<CubePopStarter>();
        if (starter != null && starter.overlayLoader != null)
        {
            loader = starter.overlayLoader;
            Debug.Log("BreathBlockBehavior: Using overlay loader assigned on CubePopStarter.");
        }

        // Next, try to find any loader in the scene if none assigned on the cube.
        if (loader == null)
        {
            loader = GameObject.FindObjectOfType<BreathingOverlayLoader>();
            if (loader != null) Debug.Log("BreathBlockBehavior: Found BreathingOverlayLoader in scene via FindObjectOfType.");
            else Debug.Log("BreathBlockBehavior: No BreathingOverlayLoader found in scene.");
        }

        if (loader != null)
        {
            loader.StartBreathingExercise();
            yield break; // loader takes responsibility for loading and starting the overlay
        }

        // Fallback: Load the breathing scene additively and position its UI in front of the player
        if (!string.IsNullOrEmpty(breathingSceneName))
        {
            var op = SceneManager.LoadSceneAsync(breathingSceneName, LoadSceneMode.Additive);
            if (op != null)
            {
                while (!op.isDone)
                {
                    yield return null;
                }

                // try to find the breathing controller in the loaded scene
                var ctrl = GameObject.FindObjectOfType<BreathingController>();
                Transform camT = null;
                if (Camera.main != null) camT = Camera.main.transform;
                else
                {
                    var cam = GameObject.FindObjectOfType<Camera>();
                    if (cam != null) camT = cam.transform;
                }

                if (ctrl != null)
                {
                    Debug.Log("BreathBlockBehavior: BreathingController found in loaded scene.");
                    if (camT != null) Debug.Log("BreathBlockBehavior: using camera " + camT.name);
                    else Debug.LogWarning("BreathBlockBehavior: no camera found for positioning.");
                    // Position the breathing canvas in front of camera, then start breathing
                    if (camT != null) ctrl.PositionCanvasInFrontOf(camT, 1.2f);

                    // animate canvas scale from 0 -> target for entrance
                    var canvasGO = GameObject.Find("BreathingCanvas");
                    if (canvasGO != null)
                    {
                        var targetScale = canvasGO.transform.localScale;
                        canvasGO.transform.localScale = Vector3.zero;
                        float anim = 0f;
                        float animDur = 0.35f;
                        while (anim < animDur)
                        {
                            anim += Time.deltaTime;
                            float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(anim / animDur));
                            canvasGO.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, p);
                            yield return null;
                        }
                        canvasGO.transform.localScale = targetScale;
                    }

                    ctrl.StartBreathing();

                    // Smoke-test visual indicator so it's obvious the scene loaded
                    if (camT != null)
                    {
                        var indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        indicator.name = "SmokeTestIndicator";
                        indicator.transform.position = camT.position + camT.forward * 0.9f + camT.up * -0.1f;
                        indicator.transform.localScale = Vector3.one * 0.12f;
                        var mr = indicator.GetComponent<Renderer>();
                        if (mr != null)
                        {
                            mr.material = new Material(Shader.Find("Unlit/Color"));
                            mr.material.color = Color.cyan;
                        }
                        Destroy(indicator, 4f);
                        Debug.Log("BreathBlockBehavior: SmokeTestIndicator created in front of camera.");
                    }
                }
            }
        }
    }
}
