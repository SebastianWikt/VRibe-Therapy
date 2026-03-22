using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Attach to a UI Button or a 3D object with a Collider.
// Clicking (UI or mouse) will load the configured scene.
public class ClickableToStart : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Name of the scene to load when clicked")]
    public string sceneName = "BreathingExercise";

    public void OnPointerClick(PointerEventData eventData)
    {
        LoadScene();
    }

    void OnMouseDown()
    {
        LoadScene();
    }

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
