using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad = "BreathingExercise";
    public string startButtonName = "StartButton";

    void Start()
    {
        var btnGO = GameObject.Find(startButtonName);
        if (btnGO != null)
        {
            var btn = btnGO.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => LoadScene());
            }
        }
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
