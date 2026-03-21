using UnityEngine;

public class SceneMoodController : MonoBehaviour
{
    [Header("References")]
    public RegulationStateManager regulationStateManager;
    public Light directionalLight;
    public Renderer waterRenderer;

    [Header("Lighting")]
    public float minLightIntensity = 0.6f;
    public float maxLightIntensity = 1.2f;

    [Header("Fog")]
    public float maxFogDensity = 0.02f;
    public float minFogDensity = 0.005f;

    [Header("Water")]
    public float minWaterSmoothness = 0.2f;
    public float maxWaterSmoothness = 0.9f;

    private void Update()
    {
        if (regulationStateManager == null) return;

        float calm = regulationStateManager.smoothedCalmScore;

        UpdateLighting(calm);
        UpdateFog(calm);
        UpdateWater(calm);
    }

    private void UpdateLighting(float calm)
    {
        if (directionalLight == null) return;

        directionalLight.intensity = Mathf.Lerp(
            minLightIntensity,
            maxLightIntensity,
            calm
        );
    }

    private void UpdateFog(float calm)
    {
        RenderSettings.fogDensity = Mathf.Lerp(
            maxFogDensity,
            minFogDensity,
            calm
        );
    }

    private void UpdateWater(float calm)
    {
        if (waterRenderer == null) return;

        float smoothness = Mathf.Lerp(
            minWaterSmoothness,
            maxWaterSmoothness,
            calm
        );

        waterRenderer.material.SetFloat("_Smoothness", smoothness);
    }
}