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
    public int textureToAvoidIndex = 0;
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

        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.alphamapResolution;

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
            
            // Sample the texture at this location
            int mapX = Mathf.FloorToInt((x / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = Mathf.FloorToInt((z / terrainData.size.z) * terrainData.alphamapHeight);
            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            if (splatmapData[0, 0, 0] > 0.9f) {

                float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.GetPosition().y - Random.Range(0, 2) - GetSlope(x,z);
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
            }
        }

        Debug.Log($"Total rocks spawned: {spawnedRocks}");
    }


    public float GetSlope(float x, float z)
    {
        // Get the height at the given position
        float height = terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.GetPosition().y;

        // Get heights at neighboring points
        float heightNorth = terrain.SampleHeight(new Vector3(x, 0, z + 1)) + terrain.GetPosition().y; // North
        float heightSouth = terrain.SampleHeight(new Vector3(x, 0, z - 1)) + terrain.GetPosition().y; // South
        float heightEast = terrain.SampleHeight(new Vector3(x + 1, 0, z)) + terrain.GetPosition().y; // East
        float heightWest = terrain.SampleHeight(new Vector3(x - 1, 0, z)) + terrain.GetPosition().y; // West

        // Calculate the slope
        float slopeX = (heightEast - heightWest) / 2; // Average slope in X direction
        float slopeZ = (heightNorth - heightSouth) / 2; // Average slope in Z direction

        // Calculate the overall slope (magnitude)
        float slope = Mathf.Sqrt(slopeX * slopeX + slopeZ * slopeZ);
        return slope; // Returns the magnitude of the slope
    }

}