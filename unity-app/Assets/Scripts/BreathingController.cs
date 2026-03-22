using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Calming breathing exercise UI inspired by common paced-breathing techniques
// (e.g., 4-4-6 inhale/hold/exhale pattern). This script creates a soft
// overlay and a circular "bubble" with a two-tone gradient + sheen to
// cue inhale, hold, and exhale phases. Not medical advice — consult a
// clinician for therapeutic protocols.
// Attach this to an empty GameObject in a Unity Scene named "BreathingExercise".
public class BreathingController : MonoBehaviour
{
    [Header("Timings (seconds)")]
    public float inhaleDuration = 4f;
    public float holdDuration = 4f;
    public float exhaleDuration = 6f;

    [Header("Presets")]
    [Tooltip("If true, the breathing loop will start automatically on Start().")]
    public bool autoStart = false;

    [Header("Visuals")]
    public Color inhaleColor = new Color(0.56f, 0.93f, 0.98f);
    public Color exhaleColor = new Color(0.2f, 0.6f, 0.8f);
    [Tooltip("Primary gradient color (e.g. lighter tone)")]
    public Color gradientColorA = new Color(0.62f, 0.92f, 0.98f);
    [Tooltip("Secondary gradient color (e.g. deeper tone)")]
    public Color gradientColorB = new Color(0.18f, 0.5f, 0.78f);
    [Tooltip("Semi-opaque background overlay to dim the scene")]
    public Color overlayColor = new Color(0f, 0f, 0f, 0.45f);
    public Vector2 circleSize = new Vector2(240, 240);
    public float outlinePadding = 10f;

    private Image circleImage;
    private Image outlineImage;
    private Image sheenImage;
    private RectTransform containerRectTransform;
    private Text phaseText;
    private Text subtitleText;

    void Start()
    {
        // Prepare UI but don't auto-start the breathing loop.
        SetupUI();
        if (autoStart) StartBreathing();
    }

    // Create a radial gradient sprite (centerColor -> edgeColor)
    private Sprite CreateRadialSprite(int size, Color centerColor, Color edgeColor)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        float maxR = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c) / maxR;
                d = Mathf.Clamp01(d);
                // smoothstep for nicer falloff
                float m = Mathf.SmoothStep(0f, 1f, d);
                Color col = Color.Lerp(centerColor, edgeColor, m);
                // fade alpha near edge
                float alpha = 1f - Mathf.SmoothStep(0.85f, 1f, d);
                col.a *= alpha;
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // Create a ring sprite where only the pixels between innerRatio and outerRatio are opaque white
    private Sprite CreateRingSprite(int size, float innerRatio, float outerRatio)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        float maxR = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c) / maxR;
                float alpha = 0f;
                // soft edge for ring
                if (d >= innerRatio && d <= outerRatio)
                    alpha = 1f - Mathf.SmoothStep(innerRatio, outerRatio, d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private Coroutine loopCoroutine;

    // Public API to start/stop the breathing exercise (can be called from UnityEvents).
    public void StartBreathing()
    {
        if (loopCoroutine == null)
        {
            Debug.Log("BreathingController: StartBreathing() called");
            if (circleImage == null || containerRectTransform == null || phaseText == null) SetupUI();
            if (circleImage != null) circleImage.enabled = true;
            if (phaseText != null) phaseText.enabled = true;
            // reset scale so the bubble starts from neutral
            if (containerRectTransform != null) containerRectTransform.localScale = Vector3.one;
            if (containerRectTransform != null) containerRectTransform.gameObject.SetActive(true);
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
        if (containerRectTransform != null) containerRectTransform.gameObject.SetActive(false);
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

        // Full-screen dim overlay
        GameObject overlayGO = new GameObject("BreathingOverlay");
        overlayGO.transform.SetParent(canvas.transform, false);
        var overlayImage = overlayGO.AddComponent<Image>();
        overlayImage.color = overlayColor;
        var overlayRect = overlayGO.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        overlayImage.raycastTarget = false;

        // Container for the bubble so we can scale/position as one unit
        GameObject container = new GameObject("BreathingContainer");
        container.transform.SetParent(canvas.transform, false);
        var containerRect = container.AddComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(800, 600);
        containerRect.anchoredPosition = Vector2.zero;
        containerRectTransform = containerRect;
        // Outline (white ring) - placed behind the gradient circle
        GameObject outlineGO = new GameObject("BreathingOutline");
        outlineGO.transform.SetParent(container.transform, false);
        outlineImage = outlineGO.AddComponent<Image>();
        outlineImage.color = Color.white;
        var outlineRect = outlineGO.GetComponent<RectTransform>();
        outlineRect.sizeDelta = circleSize + Vector2.one * outlinePadding;
        outlineRect.anchoredPosition = Vector2.zero;
        // Create procedural sprites so the bubble is circular and soft-edged
        var ringSprite = CreateRingSprite(256, 0.86f, 1.0f);
        if (ringSprite != null) outlineImage.sprite = ringSprite;
        outlineImage.type = Image.Type.Simple;

        // Gradient/Color bubble (on top)
        GameObject imgGO = new GameObject("BreathingCircle");
        imgGO.transform.SetParent(container.transform, false);
        circleImage = imgGO.AddComponent<Image>();
        circleImage.color = gradientColorA;
        var innerRect = imgGO.GetComponent<RectTransform>();
        innerRect.sizeDelta = circleSize;
        innerRect.anchoredPosition = Vector2.zero;
        var bubbleSprite = CreateRadialSprite(256, gradientColorA, gradientColorB);
        if (bubbleSprite != null) circleImage.sprite = bubbleSprite;
        circleImage.type = Image.Type.Simple;

        // Sheen - small white soft highlight to make it feel bubble-like
        GameObject sheenGO = new GameObject("BreathingSheen");
        sheenGO.transform.SetParent(container.transform, false);
        sheenImage = sheenGO.AddComponent<Image>();
        sheenImage.color = new Color(1f, 1f, 1f, 0.22f);
        var sheenRect = sheenGO.GetComponent<RectTransform>();
        sheenRect.sizeDelta = circleSize * 0.6f;
        sheenRect.anchoredPosition = new Vector2(-circleSize.x * 0.18f, circleSize.y * 0.18f);
        var sheenSprite = CreateRadialSprite(128, new Color(1f,1f,1f,1f), new Color(1f,1f,1f,0f));
        if (sheenSprite != null) sheenImage.sprite = sheenSprite;
        sheenImage.type = Image.Type.Simple;
        sheenImage.raycastTarget = false;

        // Phase text
        GameObject txtGO = new GameObject("PhaseText");
        txtGO.transform.SetParent(container.transform, false);
        phaseText = txtGO.AddComponent<Text>();
        phaseText.alignment = TextAnchor.MiddleCenter;
        Font uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (uiFont == null) uiFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
        phaseText.font = uiFont;
        phaseText.fontSize = 36;
        phaseText.fontStyle = FontStyle.Bold;
        phaseText.color = Color.white;
        var txtRect = txtGO.GetComponent<RectTransform>();
        txtRect.anchoredPosition = new Vector2(0, -circleSize.y * 0.7f);
        txtRect.sizeDelta = new Vector2(800, 90);

        // Subtitle/countdown text
        GameObject subGO = new GameObject("SubtitleText");
        subGO.transform.SetParent(container.transform, false);
        subtitleText = subGO.AddComponent<Text>();
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.font = uiFont;
        subtitleText.fontSize = 20;
        subtitleText.color = new Color(1f, 1f, 1f, 0.9f);
        var subRect = subGO.GetComponent<RectTransform>();
        subRect.anchoredPosition = new Vector2(0, -circleSize.y * 0.82f);
        subRect.sizeDelta = new Vector2(800, 60);
    }

    IEnumerator BreathLoop()
    {
        while (true)
        {
            yield return DoPhase("Inhale", inhaleDuration, 1.15f, gradientColorA, gradientColorB);
            yield return DoPhase("Hold", holdDuration, 1.0f, Color.Lerp(gradientColorA, gradientColorB, 0.5f), Color.Lerp(gradientColorA, gradientColorB, 0.5f));
            yield return DoPhase("Exhale", exhaleDuration, 0.6f, gradientColorB, gradientColorA);
        }
    }

    IEnumerator DoPhase(string name, float duration, float targetScale, Color startGradient, Color endGradient)
    {
        float t = 0f;
        Vector3 startScale = containerRectTransform.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        Color startColor = circleImage.color;
        float baseSheenAlpha = 0.22f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float raw = Mathf.Clamp01(t / duration);
            float p = Mathf.SmoothStep(0f, 1f, raw);

            // scale the whole container so outline + circle + sheen all scale together
            containerRectTransform.localScale = Vector3.Lerp(startScale, endScale, p);

            // Simulated gradient: lerp between two colors
            circleImage.color = Color.Lerp(startGradient, endGradient, p);

            // Sheen subtle alpha animation for organic feel
            if (sheenImage != null)
            {
                float sheen = Mathf.Lerp(baseSheenAlpha, baseSheenAlpha * 0.6f, p);
                var sc = sheenImage.color;
                sc.a = sheen;
                sheenImage.color = sc;
            }

            int remaining = Mathf.CeilToInt(duration - t);
            phaseText.text = string.Format("{0}", name);
            subtitleText.text = string.Format("{0}s", Mathf.Max(0, remaining));
            yield return null;
        }

        containerRectTransform.localScale = endScale;
        circleImage.color = endGradient;
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
