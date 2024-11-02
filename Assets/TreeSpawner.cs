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
            float x = Random.Range(0, terrain.terrainData.size.x);
            float z = Random.Range(0, terrain.terrainData.size.z);
            //if(x>20 && x<80 && z>80 && z<130)
            //{
            //    i--;
            //    continue;
            //}

            if((Mathf.Clamp(x,20,80)==x && Mathf.Clamp(z,80,130)==z) || (Mathf.Clamp(x,20,70)==x && Mathf.Clamp(z,20,40)==z))
            {
                i--;
                continue;
            }
            

            float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.GetPosition().y;

            // Instantiate the tree prefab
            Instantiate(treePrefab, new Vector3(x, y, z), Quaternion.identity);
        }
    }
}