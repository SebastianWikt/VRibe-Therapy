using UnityEngine;

public class DebugInput : MonoBehaviour
{
    public RegulationStateManager regulationStateManager;
    public float changeSpeed = 0.5f;

    private void Update()
    {
        if (regulationStateManager == null) return;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            regulationStateManager.rawCalmScore += changeSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            regulationStateManager.rawCalmScore -= changeSpeed * Time.deltaTime;
        }

        regulationStateManager.rawCalmScore = Mathf.Clamp01(regulationStateManager.rawCalmScore);
    }
}