using Unity.VisualScripting;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public GameObject[] treePrefabs;  // Array to hold your tree prefabs
    public int numberOfTrees;         // Number of trees to spawn
    public Terrain terrain;           // Reference to the terrain
    public float radiusMultiplier = 1.0f;  // Multiplier to adjust spawn radius based on tree size
    public LayerMask spawnLayerMask;  // Layer mask to ignore the terrain
    public int maxTries;
    void Start()
    {
        // Set spawnLayerMask to only include the "Trees" layer
        // spawnLayerMask = LayerMask.GetMask("Trees");
        Debug.Log("Spawn Layer Mask: " + spawnLayerMask.value);
        maxTries = 4000;
        SpawnTrees();

    }

    void SpawnTrees()
    {
        int spawnedTrees = 0;
        int trynum = 0;
        Debug.Log("Spawn Layer Mask: " + spawnLayerMask.value);


        for (int i = 0; i < numberOfTrees; i++)
        {
            if (trynum > maxTries) {
                break;
            }
            trynum++;
            // Randomly select a prefab from the array
            GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Length-2)];
            
            // Calculate spawn radius based on tree prefab size
            float spawnRadius = Mathf.Max(treePrefab.transform.localScale.x, treePrefab.transform.localScale.z) * radiusMultiplier;

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
                Debug.Log("Collision detected with the following objects:");
                foreach (Collider col in colliders)
                {
                    Debug.Log(col.gameObject.name + " at position " + col.transform.position);
                }
                i--;  // Retry with a new position
                continue;
            }


            // Instantiate the tree prefab
            Instantiate(treePrefab, spawnPosition, Quaternion.identity);
            spawnedTrees++;

            // Debug log to confirm tree spawn
            // Debug.Log($"Spawned tree {spawnedTrees} at position: {spawnPosition}");
        }

        // Debug.Log($"Total trees spawned: {spawnedTrees}");
    }
}
