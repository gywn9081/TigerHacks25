using UnityEngine;

public class SpawnGulagWorkers : MonoBehaviour
{
    public GameObject[] playerModels; // Array of player model prefabs
    public AnimationClip[] animations; // Array of animations to choose from
    public int numberOfPlayers = 10;   // Number of players to spawn
    public Vector3 spawnAreaSize = new Vector3(10, 0, 10); // Size of the spawn area

    void Start()
    {
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            // Choose a random player model from the array
            GameObject playerPrefab = playerModels[Random.Range(0, playerModels.Length)];
            // Instantiate the player model at a random position with a random rotation
            GameObject player = Instantiate(playerPrefab, GetRandomSpawnPosition(), GetRandomRotation());

            // Get the Animator component
            Animator animator = player.GetComponent<Animator>();
            if (animator != null && animations.Length > 0)
            {
                // Choose a random animation from the array
                AnimationClip randomAnimation = animations[Random.Range(0, animations.Length)];

                // Create a new Animation Override Controller
                var overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                overrideController[randomAnimation.name] = randomAnimation;

                // Assign the override controller back to the Animator
                animator.runtimeAnimatorController = overrideController;

                // Play the animation (assuming it's set to be triggered)
                animator.SetTrigger(randomAnimation.name); // Ensure your Animator is set up to handle triggers
            }
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float z = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);
        return new Vector3(x, 0, z) + transform.position; // Adjust based on your spawn area
    }

    Quaternion GetRandomRotation()
    {
        // Generate a random Y-axis rotation
        float randomYRotation = Random.Range(0f, 360f);
        return Quaternion.Euler(0, randomYRotation, 0); // Set Y rotation, keep X and Z at 0
    }
}