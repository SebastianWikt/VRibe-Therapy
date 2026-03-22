using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Simple breathing exercise controller that creates a minimal UI at runtime.
// Attach this to an empty GameObject in a Unity Scene named "BreathingExercise".
public class BreathingController : MonoBehaviour
{
    [Header("Timings (seconds)")]
    public float inhaleDuration = 4f;
    public float holdDuration = 4f;
    public float exhaleDuration = 6f;

    [Header("Visuals")]
    public Color inhaleColor = new Color(0.56f, 0.93f, 0.98f);
    public Color exhaleColor = new Color(0.2f, 0.6f, 0.8f);
    public Vector2 circleSize = new Vector2(240, 240);

    private Image circleImage;
    private RectTransform circleRect;
    private Text phaseText;

    void Start()
    {
        // Prepare UI but don't auto-start the breathing loop.
        SetupUI();
    }

    private Coroutine loopCoroutine;

    // Public API to start/stop the breathing exercise (can be called from UnityEvents).
    public void StartBreathing()
    {
        if (loopCoroutine == null)
        {
            Debug.Log("BreathingController: StartBreathing() called");
            if (circleImage == null || circleRect == null || phaseText == null) SetupUI();
            if (circleImage != null) circleImage.enabled = true;
            if (phaseText != null) phaseText.enabled = true;
            // reset scale so the bubble starts from neutral
            if (circleRect != null) circleRect.localScale = Vector3.one;
            loopCoroutine = StartCoroutine(BreathLoop());
        }
    }

    public void StopBreathing()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }

        if (phaseText != null) phaseText.text = string.Empty;
        if (circleImage != null) circleImage.enabled = false;
    }

    void SetupUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("BreathingCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800, 600);
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Circle image
        GameObject imgGO = new GameObject("BreathingCircle");
        imgGO.transform.SetParent(canvas.transform, false);
        circleImage = imgGO.AddComponent<Image>();
        circleImage.color = inhaleColor;
        circleRect = imgGO.GetComponent<RectTransform>();
        circleRect.sizeDelta = circleSize;
        circleRect.anchoredPosition = Vector2.zero;

        // Optional: give the image a default rounded sprite if available
        var builtin = Resources.GetBuiltinResource<Sprite>("UISprite.psd");
        if (builtin != null) circleImage.sprite = builtin;
        circleImage.type = Image.Type.Simple;

        // Phase text
        GameObject txtGO = new GameObject("PhaseText");
        txtGO.transform.SetParent(canvas.transform, false);
        phaseText = txtGO.AddComponent<Text>();
        phaseText.alignment = TextAnchor.MiddleCenter;
        phaseText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        phaseText.fontSize = 30;
        phaseText.color = Color.white;
        var txtRect = txtGO.GetComponent<RectTransform>();
        txtRect.anchoredPosition = new Vector2(0, -160);
        txtRect.sizeDelta = new Vector2(800, 80);
    }

    IEnumerator BreathLoop()
    {
        while (true)
        {
            yield return DoPhase("Inhale", inhaleDuration, 1.15f, inhaleColor);
            yield return DoPhase("Hold", holdDuration, 1.0f, Color.white);
            yield return DoPhase("Exhale", exhaleDuration, 0.6f, exhaleColor);
        }
    }

    IEnumerator DoPhase(string name, float duration, float targetScale, Color targetColor)
    {
        float t = 0f;
        Vector3 startScale = circleRect.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        Color startColor = circleImage.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            circleRect.localScale = Vector3.Lerp(startScale, endScale, p);
            circleImage.color = Color.Lerp(startColor, targetColor, p);
            int remaining = Mathf.CeilToInt(duration - t);
            phaseText.text = string.Format("{0} — {1}s", name, Mathf.Max(0, remaining));
            yield return null;
        }

        circleRect.localScale = endScale;
        circleImage.color = targetColor;
    }

    // Position the breathing UI canvas in front of the provided camera transform.
    // Converts the canvas to World Space if needed and places it at `distance` meters.
    public void PositionCanvasInFrontOf(Transform cameraTransform, float distance = 1.2f)
    {
        if (cameraTransform == null) return;
        var canvasGO = GameObject.Find("BreathingCanvas");
        if (canvasGO == null) return;

        var canvas = canvasGO.GetComponent<Canvas>();
        if (canvas == null) canvas = canvasGO.AddComponent<Canvas>();

        // Convert to World Space for VR placement
        canvas.renderMode = RenderMode.WorldSpace;
        var rect = canvasGO.GetComponent<RectTransform>();
        if (rect == null) rect = canvasGO.AddComponent<RectTransform>();

        rect.SetParent(null);
        rect.position = cameraTransform.position + cameraTransform.forward * distance;
        rect.rotation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);

        // Make the canvas reasonably sized in world units
        rect.sizeDelta = new Vector2(800, 600);
        rect.localScale = Vector3.one * 0.0025f;
    }
}
