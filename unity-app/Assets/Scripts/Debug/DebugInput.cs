using UnityEngine;
using UnityEngine.InputSystem;

public class DebugInput : MonoBehaviour
{
    public RegulationStateManager regulationStateManager;
    public float changeAmount = 0.2f;

    private void Update()
    {
        if (regulationStateManager == null || Keyboard.current == null)
            return;

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            regulationStateManager.rawCalmScore =
                Mathf.Clamp01(regulationStateManager.rawCalmScore + changeAmount);

            Debug.Log("J pressed, rawCalmScore = " + regulationStateManager.rawCalmScore);
        }

        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            regulationStateManager.rawCalmScore =
                Mathf.Clamp01(regulationStateManager.rawCalmScore - changeAmount);

            Debug.Log("K pressed, rawCalmScore = " + regulationStateManager.rawCalmScore);
        }
    }
}