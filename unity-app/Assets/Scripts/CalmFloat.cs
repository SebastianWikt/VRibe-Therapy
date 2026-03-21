using UnityEngine;

public class CalmFloat : MonoBehaviour
{
    public float amplitude = 0.1f;
    public float frequency = 0.5f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * frequency) * amplitude;
    }
}