using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    public GameObject[] animalPrefabs;  // Array to hold your tree prefabs
    public int numberOfAnimals = 10;         // Number of trees to spawn
    public Terrain terrain;           // Reference to the terrain
    public LayerMask spawnLayerMask;  // Layer mask to ignore the terrain
    public Vector3 spawnAreaSize = new Vector3(10, 0, 10); // Size of the spawn area
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnAnimals();
        UprisingMeter.Instance.GetMeterLevel();
    }

    void SpawnAnimals()
    {
        

        for(int i = 0; i < numberOfAnimals; i++)
        {
            GameObject animalPrefab = animalPrefabs[Random.Range(0, animalPrefabs.Length)];

            TerrainData terrainData = terrain.terrainData;

            float x = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float z = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);
            Vector3 spawnPosition = new Vector3(x, 0, z) + transform.position; // Adjust based on your spawn area

            GameObject newAnimal = Instantiate(animalPrefab, spawnPosition, Quaternion.identity);
            NPCController controller = newAnimal.AddComponent<NPCController>();

        }
    }
}
