using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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

    [Header("Placement")]
    [Tooltip("Drag CenterEyeAnchor here. Falls back to Camera.main if left empty.")]
    public Transform cameraOverride;
    public float canvasDistance = 1.2f;

    [Header("Presets")]
    [Tooltip("If true, the breathing loop will start automatically on Start().")]
    public bool autoStart = false;

    [Header("Visuals")]
    public Color gradientColorA = new Color(0.62f, 0.92f, 0.98f, 1f);
    public Color gradientColorB = new Color(0.18f, 0.5f, 0.78f, 1f);
    [Tooltip("Set alpha to 0 to disable dim overlay (recommended for VR)")]
    public Color overlayColor = new Color(0f, 0f, 0f, 0f);
    public Vector2 circleSize = new Vector2(240f, 240f);
    public float outlinePadding = 10f;
    public float holdPulseAmplitude = 0.0025f;
    public float holdPulseFrequency = 0.1f;
    public float holdPulseRamp = 0.25f;

    // Cached references
    private GameObject canvasGO;
    private Image circleImage;
    private Image outlineImage;
    private Image sheenImage;
    private RectTransform containerRectTransform;
    private Text phaseText;
    private Text subtitleText;
    private Font uiFontCached;

    // 3D exit button lives in world space, separate from the UI canvas
    private GameObject exitButton3D;

    [Header("Exit UI")]
    public bool destroyOnExit = false;
    public UnityEvent onExit;

    void Start()
    {
        SetupUI();

        Transform cam = cameraOverride != null
            ? cameraOverride
            : (Camera.main != null ? Camera.main.transform : null);

        PositionCanvasInFrontOf(cam, canvasDistance);

        // Position exit button now that canvas has been placed
        PositionExitButton();

        if (autoStart) StartBreathing();
    }

    // -------------------------------------------------------------------------
    // Sprite helpers
    // -------------------------------------------------------------------------

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
                float m     = Mathf.SmoothStep(0f, 1f, d);
                Color col   = Color.Lerp(centerColor, edgeColor, m);
                float alpha = 1f - Mathf.SmoothStep(0.45f, 1f, d);
                col.a *= alpha;
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

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
                if (d >= innerRatio && d <= outerRatio)
                    alpha = 1f - Mathf.SmoothStep(innerRatio, outerRatio, d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    private Coroutine loopCoroutine;

    public void StartBreathing()
    {
        if (loopCoroutine != null) return;

        Debug.Log("BreathingController: StartBreathing() called");

        if (circleImage == null || containerRectTransform == null || phaseText == null)
            SetupUI();

        if (canvasGO != null)      canvasGO.SetActive(true);
        if (circleImage != null)   circleImage.enabled = true;
        if (phaseText != null)     phaseText.enabled = true;
        if (containerRectTransform != null)
        {
            containerRectTransform.localScale = Vector3.one;
            containerRectTransform.gameObject.SetActive(true);
        }
        if (exitButton3D != null)  exitButton3D.SetActive(true);

        loopCoroutine = StartCoroutine(BreathLoop());
    }

    public void StopBreathing()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }
        if (phaseText != null)              phaseText.text = string.Empty;
        if (circleImage != null)            circleImage.enabled = false;
        if (containerRectTransform != null) containerRectTransform.gameObject.SetActive(false);
        if (exitButton3D != null)           exitButton3D.SetActive(false);
    }

    public void Exit() => OnExitButtonPressed();

    // -------------------------------------------------------------------------
    // 3D world-space exit button
    // Parented to THIS transform (BreathingExercise), not the UI canvas.
    // PositionExitButton() moves it into the right world position after the
    // canvas has been placed by PositionCanvasInFrontOf().
    // -------------------------------------------------------------------------

    private void CreateExitButton3D()
    {
        exitButton3D = new GameObject("ExitButton3D");

        // Parent to this script's GameObject so it gets destroyed with it,
        // but lives in world space (not pixel-space canvas).
        exitButton3D.transform.SetParent(this.transform, false);
        exitButton3D.transform.localScale = Vector3.one;

        // ---- Visual quad ----
        var quad  = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "ExitButtonFill";
        quad.transform.SetParent(exitButton3D.transform, false);
        quad.transform.localPosition = Vector3.zero;
        quad.transform.localRotation = Quaternion.identity;
        quad.transform.localScale    = new Vector3(0.12f, 0.05f, 1f); // 12cm x 5cm

        var rend = quad.GetComponent<Renderer>();
        rend.material       = new Material(Shader.Find("Unlit/Color"));
        rend.material.color = new Color(0.08f, 0.08f, 0.12f, 1f);

        // Remove the MeshCollider CreatePrimitive adds — we use BoxCollider instead
        var meshCol = quad.GetComponent<Collider>();
        if (meshCol != null) Destroy(meshCol);

        // ---- Text label ----
        var labelGO = new GameObject("ExitLabel");
        labelGO.transform.SetParent(exitButton3D.transform, false);
        labelGO.transform.localPosition = new Vector3(0f, 0f, -0.002f);
        labelGO.transform.localScale    = Vector3.one * 0.004f;
        var tm          = labelGO.AddComponent<TextMesh>();
        tm.text         = "Exit";
        tm.fontSize     = 32;
        tm.fontStyle    = FontStyle.Bold;
        tm.color        = Color.white;
        tm.alignment    = TextAlignment.Center;
        tm.anchor       = TextAnchor.MiddleCenter;

        // ---- Trigger collider (generous depth so finger poke registers) ----
        var col       = exitButton3D.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size      = new Vector3(0.14f, 0.07f, 0.06f);

        // ---- ProximityButton interaction ----
        var pb          = exitButton3D.AddComponent<ProximityButton>();
        pb.fillRenderer = rend;
        pb.normalColor  = new Color(0.08f, 0.08f, 0.12f, 1f);
        pb.hoveredColor = new Color(0.25f, 0.65f, 1.0f, 1f);
        pb.cooldown     = 1.5f;
        pb.onPressed.AddListener(OnExitButtonPressed);

        exitButton3D.SetActive(false);
    }

    // Place the exit button in world space below the canvas.
    // Called after PositionCanvasInFrontOf() has set the canvas position.
    private void PositionExitButton()
    {
        if (exitButton3D == null || canvasGO == null) return;

        // Canvas world position + offset: 0.18m below, same forward distance
        var canvasRect  = canvasGO.GetComponent<RectTransform>();
        Vector3 canvasWorldPos = canvasGO.transform.position;

        // "Down" in the camera's local space
        Transform cam   = cameraOverride != null
            ? cameraOverride
            : (Camera.main != null ? Camera.main.transform : null);

        Vector3 down    = cam != null ? -cam.up : Vector3.down;
        Vector3 forward = cam != null ?  cam.forward : Vector3.forward;

        // Position: below the canvas centre, slightly closer to the user
        exitButton3D.transform.position = canvasWorldPos
            + down    * 0.18f
            + forward * 0.01f; // 1cm forward so it's easy to poke

        // Face the camera
        if (cam != null)
            exitButton3D.transform.rotation = Quaternion.LookRotation(
                exitButton3D.transform.position - cam.position, cam.up);
    }

    private void OnExitButtonPressed()
    {
        StopBreathing();
        if (canvasGO != null) canvasGO.SetActive(false);
        onExit?.Invoke();
    }

    // -------------------------------------------------------------------------
    // UI construction
    // -------------------------------------------------------------------------

    void SetupUI()
    {
        if (canvasGO == null)
            canvasGO = GameObject.Find("BreathingCanvas");

        if (canvasGO != null)
        {
            var existingContainer = canvasGO.transform.Find("BreathingContainer");
            if (existingContainer != null)
            {
                containerRectTransform = existingContainer.GetComponent<RectTransform>();
                circleImage   = existingContainer.Find("BreathingCircle")?.GetComponent<Image>();
                outlineImage  = existingContainer.Find("BreathingOutline")?.GetComponent<Image>();
                sheenImage    = existingContainer.Find("BreathingSheen")?.GetComponent<Image>();
                phaseText     = existingContainer.Find("PhaseText")?.GetComponent<Text>();
                subtitleText  = existingContainer.Find("SubtitleText")?.GetComponent<Text>();
                var eb        = this.transform.Find("ExitButton3D");
                if (eb != null) exitButton3D = eb.gameObject;
                return;
            }
        }

        // ------------------------------------------------------------------
        // Canvas — 800x600 px · scale 0.0005 → 0.4m x 0.3m in world space
        // ------------------------------------------------------------------
        canvasGO              = new GameObject("BreathingCanvas");
        Canvas canvas         = canvasGO.AddComponent<Canvas>();
        canvas.renderMode     = RenderMode.WorldSpace;
        canvas.worldCamera    = Camera.main;
        canvasGO.AddComponent<CanvasScaler>();
        // No GraphicRaycaster needed — interaction via physics collider

        var canvasRect        = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta  = new Vector2(800f, 600f);
        canvasRect.localScale = Vector3.one * 0.0005f;

        // Font
        Font uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (uiFont == null) uiFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
        uiFontCached = uiFont;

        // Dim overlay
        GameObject overlayGO       = new GameObject("BreathingOverlay");
        overlayGO.transform.SetParent(canvas.transform, false);
        var overlayImage           = overlayGO.AddComponent<Image>();
        overlayImage.color         = overlayColor;
        overlayImage.raycastTarget = false;
        var overlayRect            = overlayGO.GetComponent<RectTransform>();
        overlayRect.anchorMin      = Vector2.zero;
        overlayRect.anchorMax      = Vector2.one;
        overlayRect.sizeDelta      = Vector2.zero;

        // Bubble container
        GameObject container       = new GameObject("BreathingContainer");
        container.transform.SetParent(canvas.transform, false);
        var containerRect          = container.AddComponent<RectTransform>();
        containerRect.anchorMin    = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax    = new Vector2(0.5f, 0.5f);
        containerRect.pivot        = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta    = new Vector2(800f, 600f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRectTransform     = containerRect;

        // Outline ring
        GameObject outlineGO       = new GameObject("BreathingOutline");
        outlineGO.transform.SetParent(container.transform, false);
        outlineImage               = outlineGO.AddComponent<Image>();
        outlineImage.color         = Color.white;
        var outlineRect            = outlineGO.GetComponent<RectTransform>();
        outlineRect.sizeDelta      = circleSize + Vector2.one * outlinePadding;
        outlineRect.anchoredPosition = Vector2.zero;
        var ringSprite = CreateRingSprite(256, 0.86f, 1.0f);
        if (ringSprite != null) { outlineImage.sprite = ringSprite; outlineImage.type = Image.Type.Simple; }

        // Gradient bubble
        GameObject imgGO           = new GameObject("BreathingCircle");
        imgGO.transform.SetParent(container.transform, false);
        circleImage                = imgGO.AddComponent<Image>();
        circleImage.color          = Color.white;
        var innerRect              = imgGO.GetComponent<RectTransform>();
        innerRect.sizeDelta        = circleSize;
        innerRect.anchoredPosition = Vector2.zero;
        var bubbleSprite = CreateRadialSprite(256, gradientColorA, gradientColorB);
        if (bubbleSprite != null) { circleImage.sprite = bubbleSprite; circleImage.type = Image.Type.Simple; }

        // Sheen
        GameObject sheenGO         = new GameObject("BreathingSheen");
        sheenGO.transform.SetParent(container.transform, false);
        sheenImage                 = sheenGO.AddComponent<Image>();
        sheenImage.color           = new Color(1f, 1f, 1f, 0.35f);
        sheenImage.raycastTarget   = false;
        var sheenRect              = sheenGO.GetComponent<RectTransform>();
        sheenRect.sizeDelta        = circleSize * 0.5f;
        sheenRect.anchoredPosition = new Vector2(-circleSize.x * 0.18f, circleSize.y * 0.18f);
        var sheenSprite = CreateRadialSprite(128, new Color(1f, 1f, 1f, 1f), new Color(1f, 1f, 1f, 0f));
        if (sheenSprite != null) { sheenImage.sprite = sheenSprite; sheenImage.type = Image.Type.Simple; }

        // Phase label
        GameObject txtGO           = new GameObject("PhaseText");
        txtGO.transform.SetParent(container.transform, false);
        phaseText                  = txtGO.AddComponent<Text>();
        phaseText.alignment        = TextAnchor.MiddleCenter;
        phaseText.font             = uiFont;
        phaseText.fontSize         = 36;
        phaseText.fontStyle        = FontStyle.Bold;
        phaseText.color            = Color.white;
        var txtRect                = txtGO.GetComponent<RectTransform>();
        txtRect.anchoredPosition   = new Vector2(0f, -circleSize.y * 0.7f);
        txtRect.sizeDelta          = new Vector2(300f, 60f);

        // Countdown subtitle
        GameObject subGO           = new GameObject("SubtitleText");
        subGO.transform.SetParent(container.transform, false);
        subtitleText               = subGO.AddComponent<Text>();
        subtitleText.alignment     = TextAnchor.MiddleCenter;
        subtitleText.font          = uiFont;
        subtitleText.fontSize      = 24;
        subtitleText.color         = new Color(1f, 1f, 1f, 0.9f);
        var subRect                = subGO.GetComponent<RectTransform>();
        subRect.anchoredPosition   = new Vector2(0f, -circleSize.y * 0.92f);
        subRect.sizeDelta          = new Vector2(300f, 40f);

        // 3D exit button — parented to this script's transform, not the canvas
        CreateExitButton3D();

        // Hide until StartBreathing() is called
        canvasGO.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Breath loop
    // -------------------------------------------------------------------------

    IEnumerator BreathLoop()
    {
        while (true)
        {
            yield return DoPhase("Inhale", inhaleDuration, 1.15f,
                gradientColorA, gradientColorB);
            yield return DoPhase("Hold", holdDuration, 1.0f,
                Color.Lerp(gradientColorA, gradientColorB, 0.5f),
                Color.Lerp(gradientColorA, gradientColorB, 0.5f));
            yield return DoPhase("Exhale", exhaleDuration, 0.6f,
                gradientColorB, gradientColorA);
        }
    }

    IEnumerator DoPhase(string name, float duration, float targetScale,
                        Color startGradient, Color endGradient)
    {
        float   t              = 0f;
        Vector3 startScale     = containerRectTransform.localScale;
        Vector3 endScale       = Vector3.one * targetScale;
        float   baseSheenAlpha = 0.35f;
        bool    isHold         = string.Equals(name, "Hold", System.StringComparison.OrdinalIgnoreCase);

        while (t < duration)
        {
            t += Time.deltaTime;
            float raw = Mathf.Clamp01(t / duration);
            float p   = Mathf.SmoothStep(0f, 1f, raw);

            if (isHold)
            {
                Vector3 baseScale = Vector3.Lerp(startScale, endScale, p);
                float ramp = Mathf.Min(holdPulseRamp, duration * 0.5f);
                float env  = 1f;
                if (ramp > 0f)
                {
                    float rise = Mathf.Clamp01(t / ramp);
                    float fall = Mathf.Clamp01((duration - t) / ramp);
                    env = Mathf.Min(rise, fall);
                }
                float amp   = holdPulseAmplitude * env;
                float pulse = 1f + amp * Mathf.Sin(t * Mathf.PI * 2f * holdPulseFrequency);
                containerRectTransform.localScale = baseScale * pulse;
            }
            else
            {
                containerRectTransform.localScale = Vector3.Lerp(startScale, endScale, p);
            }

            circleImage.color = Color.Lerp(startGradient, endGradient, p);

            if (sheenImage != null)
            {
                float sheen = Mathf.Lerp(baseSheenAlpha, baseSheenAlpha * 0.6f, p);
                var sc = sheenImage.color;
                sc.a = sheen;
                sheenImage.color = sc;
            }

            int remaining     = Mathf.CeilToInt(duration - t);
            phaseText.text    = name;
            subtitleText.text = string.Format("{0}s", Mathf.Max(0, remaining));
            yield return null;
        }

        containerRectTransform.localScale = endScale;
        circleImage.color = endGradient;
    }

    // -------------------------------------------------------------------------
    // Canvas placement
    // -------------------------------------------------------------------------

    public void PositionCanvasInFrontOf(Transform cameraTransform, float distance = 1.2f)
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning("BreathingController: no camera transform. Assign CenterEyeAnchor to Camera Override.");
            return;
        }

        if (canvasGO == null)
            canvasGO = GameObject.Find("BreathingCanvas");
        if (canvasGO == null) return;

        var canvas = canvasGO.GetComponent<Canvas>();
        if (canvas == null) canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        var rect = canvasGO.GetComponent<RectTransform>();
        if (rect == null) rect = canvasGO.AddComponent<RectTransform>();

        rect.SetParent(cameraTransform, false);
        rect.pivot         = new Vector2(0.5f, 0.5f);
        rect.anchorMin     = new Vector2(0.5f, 0.5f);
        rect.anchorMax     = new Vector2(0.5f, 0.5f);
        rect.localPosition = new Vector3(0f, 0f, distance);
        rect.localRotation = Quaternion.identity;
        // sizeDelta and localScale intentionally not touched
    }
}