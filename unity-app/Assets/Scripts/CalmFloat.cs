using UnityEngine;

public class CalmFloat : MonoBehaviour
{
    public float amplitude = 0.1f;
    public float frequency = 0.5f;
    private float start_seed;
    private Vector3 startPos;

    void Start()
    {
        // randomize the starting phase so instances don't float identically
        start_seed = Random.Range(0f, Mathf.PI * 2f);
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + Vector3.up * Mathf.Sin((Time.time + start_seed) * frequency) * amplitude;
    }
}