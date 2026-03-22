using UnityEngine;

public class SceneMoodController : MonoBehaviour
{
    public RegulationStateManager regulationStateManager;
    public Light directionalLight;
    public Camera sceneCamera;

    [Header("Light Intensity")]
    public float minLightIntensity = 0.45f;
    public float maxLightIntensity = 4.5f;

    [Header("Fog")]
    public float maxFogDensity = 0.10f;
    public float minFogDensity = 0.005f;

    [Header("Rain")]
    public ParticleSystem rainParticleSystem;
    public float maxRainEmission = 350f;

    [Header("Audio")]
    public AudioSource rainAudioSource;
    public AudioSource birdAudioSource;
    [Range(0f, 1f)] public float maxRainVolume = 0.8f;
    [Range(0f, 1f)] public float maxBirdVolume = 0.7f;

    [Header("Smoothing")]
    public float visualLerpSpeed = 2.5f;
    public float audioLerpSpeed = 2.0f;

    private Color currentFogColor;
    private Color currentBackgroundColor;
    private Color currentLightColor;
    private float currentLightIntensity;
    private float currentFogDensity;
    private float currentRainVolume;
    private float currentBirdVolume;

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
        else
        {
            currentLightIntensity = minLightIntensity;
            currentLightColor = Color.white;
        }

        currentFogColor = RenderSettings.fogColor;
        currentFogDensity = RenderSettings.fogDensity;

        if (sceneCamera != null)
        {
            currentBackgroundColor = sceneCamera.backgroundColor;
        }
        else
        {
            currentBackgroundColor = Color.black;
        }

        if (rainAudioSource != null)
        {
            rainAudioSource.loop = true;
            currentRainVolume = rainAudioSource.volume;

            if (!rainAudioSource.isPlaying)
                rainAudioSource.Play();
        }

        if (birdAudioSource != null)
        {
            birdAudioSource.loop = true;
            currentBirdVolume = birdAudioSource.volume;

            if (!birdAudioSource.isPlaying)
                birdAudioSource.Play();
        }

        UpdateRainVisualsImmediate(0f);
    }

    private void Update()
    {
        if (regulationStateManager == null) return;

        float calm = Mathf.Clamp01(regulationStateManager.smoothedCalmScore);

        // Keeps the visual change stronger near the darker end
        float exaggeratedCalm = Mathf.Pow(calm, 2.2f);

        // 0 = dark purple / rain
        // 1 = blue / birds
        float darkWeight = 1f - exaggeratedCalm;

        float targetLightIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, exaggeratedCalm);
        float targetFogDensity = Mathf.Lerp(maxFogDensity, minFogDensity, exaggeratedCalm);

        // Base palette: dark purple -> blue
        Color darkLightColor = new Color(0.42f, 0.34f, 0.58f);
        Color calmLightColor = new Color(0.62f, 0.82f, 1.00f);
        Color targetLightColor = Color.Lerp(darkLightColor, calmLightColor, exaggeratedCalm);

        Color darkFogColor = new Color(0.15f, 0.08f, 0.24f);
        Color calmFogColor = new Color(0.40f, 0.56f, 0.80f);
        Color targetFogColor = Color.Lerp(darkFogColor, calmFogColor, exaggeratedCalm);

        Color darkBackgroundColor = new Color(0.07f, 0.03f, 0.14f);
        Color calmBackgroundColor = new Color(0.26f, 0.46f, 0.74f);
        Color targetBackgroundColor = Color.Lerp(darkBackgroundColor, calmBackgroundColor, exaggeratedCalm);

        switch (regulationStateManager.currentState)
        {
            case RegulationState.Unknown:
                targetLightIntensity *= 0.35f;
                targetLightColor = new Color(0.35f, 0.35f, 0.42f);
                targetFogDensity = Mathf.Max(targetFogDensity, 0.08f);
                targetFogColor = new Color(0.06f, 0.05f, 0.10f);
                targetBackgroundColor = new Color(0.02f, 0.02f, 0.05f);
                break;

            case RegulationState.Unsettled:
                targetLightIntensity *= 0.60f;
                targetLightColor = Color.Lerp(targetLightColor, new Color(0.32f, 0.22f, 0.45f), 0.55f);
                targetFogDensity = Mathf.Max(targetFogDensity, 0.085f);
                targetFogColor = Color.Lerp(targetFogColor, new Color(0.10f, 0.06f, 0.16f), 0.65f);
                targetBackgroundColor = Color.Lerp(targetBackgroundColor, new Color(0.05f, 0.03f, 0.10f), 0.65f);
                break;

            case RegulationState.Settling:
                targetLightIntensity *= 0.82f;
                targetLightColor = Color.Lerp(targetLightColor, new Color(0.50f, 0.42f, 0.65f), 0.25f);
                targetFogColor = Color.Lerp(targetFogColor, new Color(0.22f, 0.20f, 0.32f), 0.25f);
                break;

            case RegulationState.Calm:
                targetLightIntensity *= 1.05f;
                targetLightColor = Color.Lerp(targetLightColor, new Color(0.68f, 0.88f, 1.00f), 0.30f);
                break;

            case RegulationState.FocusedCalm:
                targetLightIntensity *= 1.18f;
                targetLightColor = new Color(0.86f, 0.95f, 1.00f);
                targetFogDensity *= 0.50f;
                targetFogColor = new Color(0.62f, 0.76f, 0.92f);
                targetBackgroundColor = new Color(0.40f, 0.60f, 0.85f);
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
            new Color(0.08f, 0.05f, 0.12f),   // dark purple ambient
            new Color(0.20f, 0.26f, 0.36f),   // cool blue ambient
            exaggeratedCalm
        );

        if (sceneCamera != null)
        {
            sceneCamera.backgroundColor = currentBackgroundColor;
        }

        UpdateRainVisuals(darkWeight);
        UpdateAudio(darkWeight, exaggeratedCalm);
    }

    private void UpdateRainVisuals(float darkWeight)
    {
        if (rainParticleSystem == null) return;

        var emission = rainParticleSystem.emission;
        float targetRainEmission = Mathf.Lerp(0f, maxRainEmission, darkWeight);
        emission.rateOverTime = targetRainEmission;

        if (targetRainEmission > 1f)
        {
            if (!rainParticleSystem.isPlaying)
                rainParticleSystem.Play();
        }
        else
        {
            if (rainParticleSystem.isPlaying)
                rainParticleSystem.Stop();
        }
    }

    private void UpdateRainVisualsImmediate(float darkWeight)
    {
        if (rainParticleSystem == null) return;

        var emission = rainParticleSystem.emission;
        emission.rateOverTime = Mathf.Lerp(0f, maxRainEmission, darkWeight);
    }

    private void UpdateAudio(float darkWeight, float calmWeight)
    {
        if (rainAudioSource != null)
        {
            float targetRainVolume = Mathf.Lerp(0f, maxRainVolume, darkWeight);
            currentRainVolume = Mathf.Lerp(currentRainVolume, targetRainVolume, audioLerpSpeed * Time.deltaTime);
            rainAudioSource.volume = currentRainVolume;
        }

        if (birdAudioSource != null)
        {
            float targetBirdVolume = Mathf.Lerp(0f, maxBirdVolume, calmWeight);
            currentBirdVolume = Mathf.Lerp(currentBirdVolume, targetBirdVolume, audioLerpSpeed * Time.deltaTime);
            birdAudioSource.volume = currentBirdVolume;
        }
    }
}