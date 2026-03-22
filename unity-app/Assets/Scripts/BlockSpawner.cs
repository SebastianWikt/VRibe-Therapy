using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Tooltip("Assign your block prefab here.")]
    public GameObject blockPrefab;
    [Tooltip("Where to spawn new blocks.")]
    public Transform spawnPoint;
    [Tooltip("Reference to ResetBlocks script to register spawned blocks.")]
    public ResetBlocks resetBlocks; // Assign in Inspector

    public void SpawnBlock()
    {
        if (blockPrefab != null && spawnPoint != null)
        {
            GameObject newBlock = Instantiate(blockPrefab, spawnPoint.position, spawnPoint.rotation);
            if (resetBlocks != null)
            {
                resetBlocks.RegisterSpawnedBlock(newBlock);
            }
        }
    }
}
