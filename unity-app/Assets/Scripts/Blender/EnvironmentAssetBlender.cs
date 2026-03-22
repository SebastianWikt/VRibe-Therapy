using UnityEngine;

public class EnvironmentAssetBlender : MonoBehaviour
{
    public RegulationStateManager regulationStateManager;

    [Header("Environment Parents")]
    public GameObject lowMoodEnvironment;
    public GameObject highMoodEnvironment;

    [Header("Blend")]
    [Range(0f, 1f)]
    public float debugBlend = 0f;   // used if no manager is assigned
    public float blendLerpSpeed = 2f;

    private float currentBlend = 0f;

    private Renderer[] lowRenderers;
    private Renderer[] highRenderers;

    private void Start()
    {
        if (lowMoodEnvironment != null)
            lowRenderers = lowMoodEnvironment.GetComponentsInChildren<Renderer>(true);

        if (highMoodEnvironment != null)
            highRenderers = highMoodEnvironment.GetComponentsInChildren<Renderer>(true);

        PrepareMaterials(lowRenderers);
        PrepareMaterials(highRenderers);

        ApplyBlendImmediate(currentBlend);
    }

    private void Update()
    {
        float targetBlend = debugBlend;

        if (regulationStateManager != null)
        {
            // Replace this line if your score variable has a different name.
            targetBlend = Mathf.Clamp01(regulationStateManager.rawCalmScore);
        }

        currentBlend = Mathf.Lerp(currentBlend, targetBlend, Time.deltaTime * blendLerpSpeed);

        ApplyBlendImmediate(currentBlend);
    }

    private void ApplyBlendImmediate(float blend)
    {
        // low set: visible at 0, invisible at 1
        SetRendererAlpha(lowRenderers, 1f - blend);

        // high set: invisible at 0, visible at 1
        SetRendererAlpha(highRenderers, blend);
    }

    private void SetRendererAlpha(Renderer[] renderers, float alpha)
    {
        if (renderers == null) return;

        foreach (Renderer r in renderers)
        {
            if (r == null) continue;

            foreach (Material mat in r.materials)
            {
                if (mat == null) continue;

                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
            }

            // Optional: disable fully invisible renderers
            r.enabled = alpha > 0.01f;
        }
    }

    private void PrepareMaterials(Renderer[] renderers)
    {
        if (renderers == null) return;

        foreach (Renderer r in renderers)
        {
            if (r == null) continue;

            // Accessing r.materials forces unique material instances
            // so you don't accidentally edit shared project materials.
            Material[] mats = r.materials;

            foreach (Material mat in mats)
            {
                if (mat == null) continue;

                // This only works automatically if your material/shader
                // already supports transparency.
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    mat.color = new Color(c.r, c.g, c.b, c.a);
                }
            }
        }
    }
}