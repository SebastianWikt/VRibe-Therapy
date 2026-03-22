using UnityEngine;
using UnityEngine.Events;

public class PhysicalButton : MonoBehaviour
{
    [Tooltip("Event to invoke when the button is pressed (e.g., call ResetBlocks.ResetAllBlocks)")]
    public UnityEvent OnPressed;

    private bool pressed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!pressed)
        {
            pressed = true;
            OnPressed.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        pressed = false;
    }
}
