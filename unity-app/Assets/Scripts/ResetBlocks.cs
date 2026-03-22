using UnityEngine;
using System.Collections.Generic;

public class ResetBlocks : MonoBehaviour
{
    [Tooltip("Assign all building block GameObjects here.")]
    public GameObject[] blocks;

    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;

    // List to track spawned blocks
    private List<GameObject> spawnedBlocks = new List<GameObject>();

    void Start()
    {
        // Store original positions and rotations
        originalPositions = new Vector3[blocks.Length];
        originalRotations = new Quaternion[blocks.Length];
        for (int i = 0; i < blocks.Length; i++)
        {
            originalPositions[i] = blocks[i].transform.position;
            originalRotations[i] = blocks[i].transform.rotation;
        }
    }

    // Call this from BlockSpawner after instantiating a new block
    public void RegisterSpawnedBlock(GameObject block)
    {
        spawnedBlocks.Add(block);
    }

    public void ResetAllBlocks()
    {
        // Reset original blocks
        for (int i = 0; i < blocks.Length; i++)
        {
            var rb = blocks[i].GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Ensure physics is enabled
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.transform.position = originalPositions[i];
                rb.transform.rotation = originalRotations[i];
                rb.Sleep();
            }
            else
            {
                blocks[i].transform.position = originalPositions[i];
                blocks[i].transform.rotation = originalRotations[i];
            }
        }

        // Destroy all spawned blocks
        foreach (var block in spawnedBlocks)
        {
            if (block != null)
                Destroy(block);
        }
        spawnedBlocks.Clear();
    }
}
