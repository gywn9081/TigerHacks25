using Unity.VisualScripting;
using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    public GameObject[] rockPrefabs;  // Array to hold your tree prefabs
    public int numberOfRocks;         // Number of trees to spawn
    public Terrain terrain;           // Reference to the terrain
    public float radiusMultiplier = 1.0f;  // Multiplier to adjust spawn radius based on tree size
    public LayerMask spawnLayerMask;  // Layer mask to ignore the terrain
    public int maxTries = 400;
    void Start()
    {
        // Set spawnLayerMask to only include the "Trees" layer
        // spawnLayerMask = LayerMask.GetMask("Trees");
        Debug.Log("Spawn Layer Mask: " + spawnLayerMask.value);
        SpawnRocks();

    }
    void SpawnRocks()
    {
        int spawnedRocks = 0;
        int trynum = 0;
        Debug.Log("Spawn Layer Mask: " + spawnLayerMask.value);


        for (int i = 0; i < numberOfRocks; i++)
        {
            if (trynum > maxTries) {
                break;
            }
            trynum++;
            // Randomly select a prefab from the array
            GameObject rockPrefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
            
            // Calculate spawn radius based on rock prefab size
            float spawnRadius = Mathf.Max(rockPrefab.transform.localScale.x, rockPrefab.transform.localScale.z) * radiusMultiplier;

            // Generate random position within the terrain bounds
            float x = Random.Range(0, terrain.terrainData.size.x);
            float z = Random.Range(0, terrain.terrainData.size.z);

            // Check if the position is within restricted zones
            if ((Mathf.Clamp(x, 20, 80) == x && Mathf.Clamp(z, 80, 130) == z) || 
                (Mathf.Clamp(x, 20, 70) == x && Mathf.Clamp(z, 20, 40) == z))
            {
                i--;  // Retry with a new position
                continue;
            }

            float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.GetPosition().y;
            Vector3 spawnPosition = new Vector3(x, y, z);

            // Check for any existing objects within the dynamic spawn radius, excluding the terrain
            Collider[] colliders = Physics.OverlapSphere(spawnPosition, spawnRadius, spawnLayerMask);
            if (colliders.Length > 0)
            {
                Debug.Log("Collision detected");
                i--;  // Retry with a new position
                continue;
            }


            // Instantiate the rock prefab
            Instantiate(rockPrefab, spawnPosition, Quaternion.identity);
            spawnedRocks++;

            // Debug log to confirm rock spawn
            // Debug.Log($"Spawned rocks {spawnedRocks} at position: {spawnPosition}");
        }

        Debug.Log($"Total rocks spawned: {spawnedRocks}");
    }


}