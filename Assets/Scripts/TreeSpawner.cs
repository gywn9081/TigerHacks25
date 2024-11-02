using Unity.VisualScripting;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public GameObject[] treePrefabs;  // Array to hold your tree prefabs
    public int numberOfTrees;         // Number of trees to spawn
    public Terrain terrain;           // Reference to the terrain
    public float radiusMultiplier = 1.0f;  // Multiplier to adjust spawn radius based on tree size
    public LayerMask spawnLayerMask;  // Layer mask to ignore the terrain
    public int maxTries = 400;
    public int textureToAvoidIndex = 0;
    void Start()
    {
        // Set spawnLayerMask to only include the "Trees" layer
        // spawnLayerMask = LayerMask.GetMask("Trees");
        Debug.Log("Spawn Layer Mask: " + spawnLayerMask.value);
        SpawnTrees();

    }

    void SpawnTrees()
    {
        int spawnedTrees = 0;
        int trynum = 0;
        Debug.Log("Spawn Layer Mask: " + spawnLayerMask.value);

        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.alphamapResolution;

        for (int i = 0; i < numberOfTrees; i++)
        {
            if (trynum > maxTries) {
                break;
            }
            trynum++;
            // Randomly select a prefab from the array
            GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
            
            // Calculate spawn radius based on tree prefab size
            float spawnRadius = Mathf.Max(treePrefab.transform.localScale.x, treePrefab.transform.localScale.z) * radiusMultiplier;

            // Generate random position within the terrain bounds
            float x = Random.Range(0, terrain.terrainData.size.x);
            float z = Random.Range(0, terrain.terrainData.size.z);

            // Sample the texture at this location
            int mapX = Mathf.FloorToInt((x / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = Mathf.FloorToInt((z / terrainData.size.z) * terrainData.alphamapHeight);
            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);


            // Check if the texture to avoid is dominant
            if (splatmapData[0, 0, 0] > 0.9f) // Adjust threshold as needed
            {
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


                // Instantiate the tree prefab
                Instantiate(treePrefab, spawnPosition, Quaternion.identity);
                spawnedTrees++;
            }
        }

        Debug.Log($"Total trees spawned: {spawnedTrees}");
    }
}
