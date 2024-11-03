using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    public GameObject[] animalPrefabs;  // Array to hold your tree prefabs
    public int numberOfAnimals = 10;         // Number of trees to spawn
    public Terrain terrain;           // Reference to the terrain
    public LayerMask spawnLayerMask;  // Layer mask to ignore the terrain
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnAnimals();
    }

    void SpawnAnimals()
    {
        

        for(int i = 0; i < numberOfAnimals; i++)
        {
            GameObject animalPrefab = animalPrefabs[Random.Range(0, animalPrefabs.Length)];

            TerrainData terrainData = terrain.terrainData;

            float x = Random.Range(0, terrain.terrainData.size.x);
            float z = Random.Range(0, terrain.terrainData.size.z);
            float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.GetPosition().y + 1;

            Vector3 spawnPosition = new Vector3(x, y, z);

            GameObject newAnimal = Instantiate(animalPrefab, spawnPosition, Quaternion.identity);
            NPCController controller = newAnimal.AddComponent<NPCController>();

        }
    }
}
