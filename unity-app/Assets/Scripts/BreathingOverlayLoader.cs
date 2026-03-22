using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Parent-side helper: load breathing overlay scene additively, start the exercise,
// show an Exit button in the parent canvas, and safely unload the overlay when finished.
public class BreathingOverlayLoader : MonoBehaviour
{
    [Tooltip("Name of the scene that contains the BreathingController (additive overlay)")]
    public string breathingSceneName = "BreathingScene";
    [Tooltip("Small delay before unloading overlay to let XR subsystems quiesce")]
    public float unloadDelay = 0.6f;
    [Tooltip("If true, create a parent-scene Exit button instead of using the overlay's button")]
    public bool createParentExitButton = true;
    [Header("Prefab Fallback")]
    [Tooltip("Optional: assign a prefab (or root GameObject) containing the breathing overlay (BreathingController). If set, the loader will instantiate this prefab instead of loading an additive scene.")]
    public GameObject breathingOverlayPrefab;

    private Scene overlayScene;
    private BreathingController controller;
    private GameObject parentExitPanel;
    // If using prefab instantiation, keep the instantiated root here so we can destroy it on exit
    private GameObject instantiatedOverlayRoot;
    // Optional: object to hide while the overlay is active (e.g. the popped cube)
    private GameObject hiddenObject;
    private bool hiddenObjectWasActive = false;

    // Call this to start the breathing overlay (e.g., from your cube pop interaction)
    public void StartBreathingExercise()
    {
        // backward-compatible start without hiding a target
        hiddenObject = null;
        hiddenObjectWasActive = false;
        StartCoroutine(LoadAndStart());
    }

    // Call this to start the breathing overlay and hide `hideTarget` until the overlay exits
    public void StartBreathingExerciseFor(GameObject hideTarget)
    {
        hiddenObject = hideTarget;
        hiddenObjectWasActive = (hiddenObject != null) ? hiddenObject.activeSelf : false;
        StartCoroutine(LoadAndStart());
    }

    private IEnumerator LoadAndStart()
    {
        // If a prefab is assigned, instantiate it directly in the parent scene and use that as the overlay
        if (breathingOverlayPrefab != null)
        {
            Debug.Log("BreathingOverlayLoader: Instantiating assigned overlay prefab.");
            instantiatedOverlayRoot = Instantiate(breathingOverlayPrefab, Vector3.zero, Quaternion.identity);
            instantiatedOverlayRoot.name = breathingOverlayPrefab.name + "_Instance";
            controller = instantiatedOverlayRoot.GetComponentInChildren<BreathingController>();
            if (controller == null)
            {
                Debug.LogWarning("BreathingOverlayLoader: prefab does not contain a BreathingController in children.");
                yield break;
            }
            Debug.Log("BreathingOverlayLoader: BreathingController found in prefab instance. Preparing overlay.");
        }
        else
        {
            Debug.Log("BreathingOverlayLoader: Loading scene '" + breathingSceneName + "' additively.");
            var op = SceneManager.LoadSceneAsync(breathingSceneName, LoadSceneMode.Additive);
            if (op == null)
            {
                Debug.LogWarning("BreathingOverlayLoader: Failed to start loading breathing scene: " + breathingSceneName);
                yield break;
            }

            while (!op.isDone) yield return null;

            overlayScene = SceneManager.GetSceneByName(breathingSceneName);
            if (!overlayScene.IsValid())
            {
                Debug.LogWarning("BreathingOverlayLoader: Breathing overlay scene not found after load: " + breathingSceneName);
                // Log currently loaded scenes for debugging
                for (int i = 0; i < SceneManager.sceneCount; i++) Debug.Log("Loaded scene: " + SceneManager.GetSceneAt(i).name);
                yield break;
            }

            Debug.Log("BreathingOverlayLoader: Scene loaded: " + overlayScene.name + ", searching for BreathingController...");

            // Find the BreathingController in the loaded scene (search root objects first)
            foreach (var root in overlayScene.GetRootGameObjects())
            {
                Debug.Log("BreathingOverlayLoader: root object in overlay scene: " + root.name);
                controller = root.GetComponentInChildren<BreathingController>();
                if (controller != null) break;
            }

            // Fallback: try a global Find if not found in roots (in case controller was placed under DontDestroy or similar)
            if (controller == null)
            {
                controller = GameObject.FindObjectOfType<BreathingController>();
                if (controller != null) Debug.Log("BreathingOverlayLoader: Found BreathingController via global FindObjectOfType.");
            }

            if (controller == null)
            {
                Debug.LogWarning("BreathingOverlayLoader: BreathingController not found in overlay scene: " + breathingSceneName);
                yield break;
            }

            Debug.Log("BreathingOverlayLoader: BreathingController found. Preparing overlay.");
        }

        // Defensive: make sure overlay doesn't try to unload itself
        controller.destroyOnExit = false;

        // Optionally create a parent Exit button and hide overlay's own exit UI
        if (createParentExitButton)
        {
            CreateParentExitUI();
            // try to hide overlay's exit panel if present
            TryHideOverlayExitPanel();
        }

        // Subscribe for exit event (safe guard)
        try
        {
            controller.onExit.AddListener(OnOverlayExit);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("BreathingOverlayLoader: Failed to subscribe to controller.onExit: " + ex);
        }

        // Position canvas in front of camera if possible
        try
        {
            Transform camT = null;
            if (Camera.main != null) camT = Camera.main.transform;
            else
            {
                var cam = GameObject.FindObjectOfType<Camera>();
                if (cam != null) camT = cam.transform;
            }
            if (camT != null)
            {
                controller.PositionCanvasInFrontOf(camT, 1.2f);
                Debug.Log("BreathingOverlayLoader: Positioned overlay canvas in front of camera.");
                // If we instantiated the overlay prefab, PositionCanvasInFrontOf may have
                // detached the canvas from the prefab root (it calls SetParent(null)).
                // Reparent the canvas back under the instantiated root so it will be
                // cleaned up when we Destroy(instantiatedOverlayRoot) on exit.
                if (instantiatedOverlayRoot != null)
                {
                    var canvasGO = GameObject.Find("BreathingCanvas");
                    if (canvasGO != null)
                    {
                        try { canvasGO.transform.SetParent(instantiatedOverlayRoot.transform, true); } catch { }
                    }
                }
            }
            else Debug.LogWarning("BreathingOverlayLoader: No camera found to position overlay.");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("BreathingOverlayLoader: Positioning error: " + ex);
        }

        // Start breathing
        try
        {
                // If requested, hide the caller object so it looks replaced by the overlay
                if (hiddenObject != null)
                {
                    try { hiddenObjectWasActive = hiddenObject.activeSelf; hiddenObject.SetActive(false); } catch (System.Exception ex) { Debug.LogWarning("BreathingOverlayLoader: error hiding target: " + ex); }
                }
            controller.StartBreathing();
            Debug.Log("BreathingOverlayLoader: Started breathing on controller.");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("BreathingOverlayLoader: Failed to start breathing: " + ex);
        }
    }

    private void TryHideOverlayExitPanel()
    {
        // overlay's exitPanel is private; we can attempt to find the object by name
        var panel = GameObject.Find("BreathingExitPanel");
        if (panel != null) panel.SetActive(false);
    }

    private void CreateParentExitUI()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("No Canvas found in parent scene to host Exit button.");
            return;
        }

        parentExitPanel = new GameObject("BreathingExitPanel_Parent");
        parentExitPanel.transform.SetParent(canvas.transform, false);
        var rect = parentExitPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-16f, -16f);
        rect.sizeDelta = new Vector2(140f, 48f);

        var bg = parentExitPanel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0f, 0f, 0f, 0.45f);

        GameObject btnGO = new GameObject("ExitButton_Parent");
        btnGO.transform.SetParent(parentExitPanel.transform, false);
        var btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.anchorMin = Vector2.zero;
        btnRect.anchorMax = Vector2.one;
        btnRect.sizeDelta = Vector2.zero;

        var img = btnGO.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(1f, 1f, 1f, 0.06f);
        var button = btnGO.AddComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(ParentExitPressed);

        GameObject btText = new GameObject("ExitText_Parent");
        btText.transform.SetParent(btnGO.transform, false);
        var t = btText.AddComponent<UnityEngine.UI.Text>();
        t.alignment = TextAnchor.MiddleCenter;
        t.text = "Exit";
        t.color = Color.white;
        t.fontSize = 18;
        // try to set a default font
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Font.CreateDynamicFontFromOSFont("Arial", 14);
        var tt = btText.GetComponent<RectTransform>();
        tt.anchorMin = Vector2.zero;
        tt.anchorMax = Vector2.one;
        tt.sizeDelta = Vector2.zero;

        parentExitPanel.SetActive(true);
    }

    private void ParentExitPressed()
    {
        if (controller != null)
        {
            controller.Exit();
        }
    }

    private void OnOverlayExit()
    {
        StartCoroutine(UnloadOverlaySafe());
    }

    private IEnumerator UnloadOverlaySafe()
    {
        // Immediately disable overlay root objects to stop Update/OnDisable activity
        if (instantiatedOverlayRoot != null)
        {
            try { instantiatedOverlayRoot.SetActive(false); } catch { }
        }
        else if (overlayScene.IsValid() && overlayScene.isLoaded)
        {
            try
            {
                var roots = overlayScene.GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                {
                    try { roots[i].SetActive(false); } catch { }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("BreathingOverlayLoader: error disabling overlay roots: " + ex);
            }
        }

        // Give other systems a longer moment to finish OnDisable/Updates
        yield return new WaitForSeconds(unloadDelay);

        if (instantiatedOverlayRoot != null)
        {
            // destroy instantiated prefab root
            try { Destroy(instantiatedOverlayRoot); } catch (System.Exception ex) { Debug.LogWarning("BreathingOverlayLoader: error destroying instantiated overlay: " + ex); }
            instantiatedOverlayRoot = null;
        }
        else if (overlayScene.IsValid() && overlayScene.isLoaded)
        {
            AsyncOperation unload = null;
            try
            {
                unload = SceneManager.UnloadSceneAsync(overlayScene);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("BreathingOverlayLoader: error starting UnloadSceneAsync: " + ex);
            }

            while (unload != null && !unload.isDone) yield return null;
        }

        // cleanup parent exit UI
        if (parentExitPanel != null) Destroy(parentExitPanel);

        // restore any hidden object (the cube) back to its previous active state
        if (hiddenObject != null)
        {
            try { hiddenObject.SetActive(hiddenObjectWasActive); } catch { }
            hiddenObject = null;
            hiddenObjectWasActive = false;
        }

        if (controller != null)
        {
            try { controller.onExit.RemoveListener(OnOverlayExit); } catch { }
            controller = null;
        }

        overlayScene = default;
    }
}
