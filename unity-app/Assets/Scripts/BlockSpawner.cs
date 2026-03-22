using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Tooltip("Assign your block prefab here.")]
    public GameObject blockPrefab;
    [Tooltip("Where to spawn new blocks.")]
    public Transform spawnPoint;

    public void SpawnBlock()
    {
        if (blockPrefab != null && spawnPoint != null)
        {
            Instantiate(blockPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
