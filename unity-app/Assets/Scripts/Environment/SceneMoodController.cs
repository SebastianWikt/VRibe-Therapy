using UnityEngine;

public class SceneMoodController : MonoBehaviour
{
    public RegulationStateManager regulationStateManager;
    public Light directionalLight;
    public Camera sceneCamera;

    [Header("Light Intensity")]
    public float minLightIntensity = 0.5f;
    public float maxLightIntensity = 6.0f;

    [Header("Fog")]
    public float maxFogDensity = 0.15f;
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
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        if (directionalLight != null)
        {
            currentLightIntensity = directionalLight.intensity;
            currentLightColor = directionalLight.color;
        }

        currentFogColor = RenderSettings.fogColor;
        currentFogDensity = RenderSettings.fogDensity;

        if (sceneCamera != null)
        {
            currentBackgroundColor = sceneCamera.backgroundColor;
        }
    }

    private void Update()
    {
        if (regulationStateManager == null) return;

        float calm = Mathf.Clamp01(regulationStateManager.smoothedCalmScore);
        float exaggeratedCalm = Mathf.Pow(calm, 3.5f);

        float targetLightIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, exaggeratedCalm);
        float targetFogDensity = Mathf.Lerp(maxFogDensity, minFogDensity, exaggeratedCalm);

        Color stressedLightColor = new Color(1.0f, 0.55f, 0.25f);
        Color calmLightColor = new Color(0.55f, 0.82f, 1.0f);
        Color targetLightColor = Color.Lerp(stressedLightColor, calmLightColor, exaggeratedCalm);

        Color stressedFogColor = new Color(0.35f, 0.18f, 0.10f);
        Color calmFogColor = new Color(0.45f, 0.58f, 0.72f);
        Color targetFogColor = Color.Lerp(stressedFogColor, calmFogColor, exaggeratedCalm);

        Color stressedBackgroundColor = new Color(0.65f, 0.35f, 0.15f);
        Color calmBackgroundColor = new Color(0.28f, 0.45f, 0.62f);
        Color targetBackgroundColor = Color.Lerp(stressedBackgroundColor, calmBackgroundColor, exaggeratedCalm);

        switch (regulationStateManager.currentState)
        {
            case RegulationState.Unknown:
                targetLightIntensity *= 0.2f;
                targetLightColor = new Color(0.55f, 0.55f, 0.60f);
                targetFogDensity = Mathf.Max(targetFogDensity, 0.08f);
                targetFogColor = new Color(0.05f, 0.05f, 0.07f);
                targetBackgroundColor = new Color(0.01f, 0.01f, 0.02f);
                break;

            case RegulationState.Unsettled:
                targetLightIntensity *= 0.35f;
                targetLightColor = Color.Lerp(targetLightColor, new Color(1.0f, 0.30f, 0.25f), 0.65f);
                targetFogDensity = Mathf.Max(targetFogDensity, 0.10f);
                targetFogColor = new Color(0.08f, 0.05f, 0.05f);
                targetBackgroundColor = new Color(0.05f, 0.03f, 0.04f);
                break;

            case RegulationState.Settling:
                targetLightIntensity *= 0.75f;
                targetLightColor = Color.Lerp(targetLightColor, new Color(0.95f, 0.78f, 0.50f), 0.35f);
                targetFogColor = Color.Lerp(targetFogColor, new Color(0.22f, 0.24f, 0.28f), 0.35f);
                break;

            case RegulationState.Calm:
                targetLightIntensity *= 1.1f;
                targetLightColor = Color.Lerp(targetLightColor, new Color(0.55f, 0.85f, 1.0f), 0.35f);
                break;

            case RegulationState.FocusedCalm:
                targetLightIntensity *= 1.3f;
                targetLightColor = new Color(0.85f, 0.96f, 1.0f);
                targetFogDensity *= 0.35f;
                targetFogColor = new Color(0.65f, 0.76f, 0.88f);
                targetBackgroundColor = new Color(0.38f, 0.56f, 0.74f);
                break;
        }

        currentLightIntensity = Mathf.Lerp(currentLightIntensity, targetLightIntensity, visualLerpSpeed * Time.deltaTime);
        currentFogDensity = Mathf.Lerp(currentFogDensity, targetFogDensity, visualLerpSpeed * Time.deltaTime);
        currentLightColor = Color.Lerp(currentLightColor, targetLightColor, visualLerpSpeed * Time.deltaTime);
        currentFogColor = Color.Lerp(currentFogColor, targetFogColor, visualLerpSpeed * Time.deltaTime);
        currentBackgroundColor = Color.Lerp(currentBackgroundColor, targetBackgroundColor, visualLerpSpeed * Time.deltaTime);

        if (directionalLight != null)
        {
            directionalLight.intensity = currentLightIntensity;
            directionalLight.color = currentLightColor;
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = currentFogColor;
        RenderSettings.fogDensity = currentFogDensity;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.Lerp(
            new Color(0.25f, 0.15f, 0.08f),   // warm orange ambient
            new Color(0.18f, 0.22f, 0.30f),
            exaggeratedCalm
        );

        if (sceneCamera != null)
        {
            sceneCamera.backgroundColor = currentBackgroundColor;
        }
    }
}