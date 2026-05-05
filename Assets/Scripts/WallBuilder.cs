using UnityEngine;

public class WallBuilder : MonoBehaviour
{
    [Header("Wall Settings")]
    public GameObject chunkPrefab; // The brick we want to spawn
    public int widthInBlocks = 5;
    public int heightInBlocks = 3;
    
    [Header("Spacing")]
    public float blockWidth = 1f;  // Change this if your chunk X scale isn't 1
    public float blockHeight = 1f; // Change this if your chunk Y scale isn't 1

    // This magic tag lets us run the function from the Unity Editor!
    [ContextMenu("Build Wall")]
    public void Build()
    {
        if (chunkPrefab == null)
        {
            Debug.LogWarning("Missing Chunk Prefab!");
            return;
        }

        // 1. Clear out any old blocks if we are rebuilding
        // We use DestroyImmediate because we are doing this in the Editor, not during runtime
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // 2. Build the grid
        for (int x = 0; x < widthInBlocks; x++)
        {
            for (int y = 0; y < heightInBlocks; y++)
            {
                // Calculate where the block should sit relative to this generator
                Vector3 spawnPos = transform.position + (transform.right * (x * blockWidth)) + (transform.up * (y * blockHeight));

                // Spawn it
                GameObject newBlock = Instantiate(chunkPrefab, spawnPos, transform.rotation);
                
                // Name it neatly and make it a child of this Builder object
                newBlock.name = $"Chunk_{x}_{y}";
                newBlock.transform.SetParent(this.transform);
            }
        }
    }
}