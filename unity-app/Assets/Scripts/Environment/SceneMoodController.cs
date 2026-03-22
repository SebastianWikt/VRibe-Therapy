using UnityEngine;

public class SceneMoodController : MonoBehaviour
{
    public RegulationStateManager regulationStateManager;
    public Light directionalLight;

    public float minLightIntensity = 0.05f;
    public float maxLightIntensity = 3.0f;

    public float maxFogDensity = 0.1f;
    public float minFogDensity = 0.0f;

    private void Update()
    {
        Debug.Log("SceneMoodController calm = " + regulationStateManager.smoothedCalmScore);
        if (regulationStateManager == null) return;

        float calm = regulationStateManager.smoothedCalmScore;

        if (directionalLight != null)
        {
            directionalLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, calm);
        }

        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.Lerp(
            new Color(0.45f, 0.45f, 0.5f),
            new Color(0.85f, 0.93f, 1.0f),
            calm
        );
        RenderSettings.fogDensity = Mathf.Lerp(maxFogDensity, minFogDensity, calm);

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = Color.Lerp(
                new Color(0.2f, 0.2f, 0.25f),
                new Color(0.75f, 0.9f, 1.0f),
                calm
            );
        }
    }
}