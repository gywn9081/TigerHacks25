using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public GameObject[] treePrefabs; // Array to hold your tree prefabs
    public int numberOfTrees;   // Number of trees to spawn
    public Terrain terrain;            // Reference to the terrain

    void Start()
    {
        SpawnTrees();
    }

    void SpawnTrees()
    {
        for (int i = 0; i < numberOfTrees; i++)
        {
            // Randomly select a prefab from the array
            GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Length-1)];
            
            // Generate random position within the terrain bounds
            float x = Random.Range(0, terrain.terrainData.size.x-1);
            float z = Random.Range(0, terrain.terrainData.size.z-1);
            float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.GetPosition().y;

            // Instantiate the tree prefab
            Instantiate(treePrefab, new Vector3(x, y, z), Quaternion.identity);
        }
    }
}