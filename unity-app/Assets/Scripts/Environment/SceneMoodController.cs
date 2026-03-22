using UnityEngine;

public class SceneMoodController : MonoBehaviour
{
    public RegulationStateManager regulationStateManager;
    public Light directionalLight;

    [Header("Light Intensity")]
    public float minLightIntensity = 0.05f;
    public float maxLightIntensity = 6.0f;

    [Header("Fog")]
    public float maxFogDensity = 0.06f;
    public float minFogDensity = 0.0f;

    [Header("Smoothing")]
    public float visualLerpSpeed = 2.5f;

    private Color currentFogColor;
    private Color currentBackgroundColor;
    private Color currentLightColor;
    private float currentLightIntensity;
    private float currentFogDensity;

    private void Start()
    {
        RenderSettings.fog = true;

        if (directionalLight != null)
        {
            currentLightIntensity = directionalLight.intensity;
            currentLightColor = directionalLight.color;
        }

        currentFogColor = RenderSettings.fogColor;

        Camera cam = Camera.main;
        if (cam != null)
        {
            currentBackgroundColor = cam.backgroundColor;
        }
    }

    private void Update()
    {
        if (regulationStateManager == null) return;

        float calm = regulationStateManager.smoothedCalmScore;
        calm = Mathf.Clamp01(calm);

        Debug.Log("SceneMoodController calm = " + calm);

        // Exaggerate the effect so the scene change feels stronger.
        float exaggeratedCalm = Mathf.Pow(calm, 2.2f);

        // Base targets from calm score.
        float targetLightIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, exaggeratedCalm);
        float targetFogDensity = Mathf.Lerp(maxFogDensity, minFogDensity, exaggeratedCalm);

        Color stressedLightColor = new Color(1.0f, 0.35f, 0.35f);
        Color calmLightColor = new Color(0.55f, 0.82f, 1.0f);
        Color targetLightColor = Color.Lerp(stressedLightColor, calmLightColor, exaggeratedCalm);

        Color stressedFogColor = new Color(0.28f, 0.28f, 0.32f);
        Color calmFogColor = new Color(0.82f, 0.91f, 1.0f);
        Color targetFogColor = Color.Lerp(stressedFogColor, calmFogColor, exaggeratedCalm);

        Color stressedBackgroundColor = new Color(0.12f, 0.12f, 0.16f);
        Color calmBackgroundColor = new Color(0.70f, 0.88f, 1.0f);
        Color targetBackgroundColor = Color.Lerp(stressedBackgroundColor, calmBackgroundColor, exaggeratedCalm);

        // Extra state-based styling to make each state feel more distinct.
        switch (regulationStateManager.currentState)
        {
            case RegulationState.Unknown:
                targetLightIntensity *= 0.35f;
                targetLightColor = new Color(0.55f, 0.55f, 0.60f);
                targetFogDensity = Mathf.Max(targetFogDensity, 0.05f);
                targetFogColor = new Color(0.22f, 0.22f, 0.26f);
                targetBackgroundColor = new Color(0.10f, 0.10f, 0.12f);
                break;

            case RegulationState.Unsettled:
                targetLightIntensity *= 0.55f;
                targetLightColor = Color.Lerp(targetLightColor, new Color(1.0f, 0.30f, 0.25f), 0.65f);
                targetFogDensity = Mathf.Max(targetFogDensity, 0.04f);
                targetFogColor = Color.Lerp(targetFogColor, new Color(0.35f, 0.28f, 0.28f), 0.6f);
                targetBackgroundColor = Color.Lerp(targetBackgroundColor, new Color(0.18f, 0.12f, 0.12f), 0.6f);
                break;

            case RegulationState.Settling:
                targetLightIntensity *= 0.85f;
                targetLightColor = Color.Lerp(targetLightColor, new Color(0.95f, 0.78f, 0.50f), 0.35f);
                targetFogColor = Color.Lerp(targetFogColor, new Color(0.65f, 0.68f, 0.72f), 0.25f);
                break;

            case RegulationState.Calm:
                targetLightIntensity *= 1.1f;
                targetLightColor = Color.Lerp(targetLightColor, new Color(0.55f, 0.85f, 1.0f), 0.35f);
                break;

            case RegulationState.FocusedCalm:
                targetLightIntensity *= 1.3f;
                targetLightColor = new Color(0.85f, 0.96f, 1.0f);
                targetFogDensity *= 0.35f;
                targetFogColor = new Color(0.90f, 0.96f, 1.0f);
                targetBackgroundColor = new Color(0.82f, 0.93f, 1.0f);
                break;

            case RegulationState.Overstimulated:
                targetLightIntensity *= 0.45f;
                targetLightColor = new Color(1.0f, 0.20f, 0.20f);
                targetFogDensity = 0.06f;
                targetFogColor = new Color(0.30f, 0.18f, 0.18f);
                targetBackgroundColor = new Color(0.15f, 0.08f, 0.08f);
                break;

            case RegulationState.Recovering:
                targetLightIntensity *= 0.75f;
                targetLightColor = new Color(0.85f, 0.72f, 0.55f);
                targetFogDensity *= 0.8f;
                targetFogColor = new Color(0.60f, 0.58f, 0.56f);
                targetBackgroundColor = new Color(0.42f, 0.40f, 0.42f);
                break;
        }

        // Smooth visual transitions.
        currentLightIntensity = Mathf.Lerp(currentLightIntensity, targetLightIntensity, visualLerpSpeed * Time.deltaTime);
        currentFogDensity = Mathf.Lerp(currentFogDensity, targetFogDensity, visualLerpSpeed * Time.deltaTime);
        currentLightColor = Color.Lerp(currentLightColor, targetLightColor, visualLerpSpeed * Time.deltaTime);
        currentFogColor = Color.Lerp(currentFogColor, targetFogColor, visualLerpSpeed * Time.deltaTime);
        currentBackgroundColor = Color.Lerp(currentBackgroundColor, targetBackgroundColor, visualLerpSpeed * Time.deltaTime);

        // Apply to scene.
        if (directionalLight != null)
        {
            directionalLight.intensity = currentLightIntensity;
            directionalLight.color = currentLightColor;

            // Optional: tilt the light slightly as calm rises.
            directionalLight.transform.rotation = Quaternion.Lerp(
                directionalLight.transform.rotation,
                Quaternion.Euler(Mathf.Lerp(12f, 50f, exaggeratedCalm), 30f, 0f),
                visualLerpSpeed * Time.deltaTime
            );
        }

        RenderSettings.fog = true;
        RenderSettings.fogColor = currentFogColor;
        RenderSettings.fogDensity = currentFogDensity;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.Lerp(
            new Color(0.02f, 0.02f, 0.04f),
            new Color(0.45f, 0.50f, 0.60f),
            exaggeratedCalm
        );

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = currentBackgroundColor;
        }
    }
}