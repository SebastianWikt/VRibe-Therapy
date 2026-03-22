using UnityEngine;
using UnityEngine.UI;

public class IntroPanelController : MonoBehaviour
{
    public GameObject introPanel;

    void Start()
    {
        // Show the panel at the start
        introPanel.SetActive(true);
    }

    // Call this from your button's OnClick event
    public void HidePanel()
    {
        introPanel.SetActive(false);
    }
}