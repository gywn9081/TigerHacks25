using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class LevelDoor : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName;
    [Tooltip("Leave at -1 to use scene name, or specify a build index")]
    [SerializeField] private int nextSceneBuildIndex = -1;

    [Header("Door Settings")]
    [SerializeField] private bool requiresCheckpoint = true;
    [SerializeField] private bool requiresAllPlayers = true;

    [Header("Visual Feedback")]
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem unlockParticles;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip doorEnterSound;
    [SerializeField] private AudioClip doorExitSound;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDelay = 1.0f;

    private bool isUnlocked = false;
    private bool isTransitioning = false;
    private AudioSource audioSource;

    // Track players near the door and inside the door
    private HashSet<PlayerController> playersNearDoor = new HashSet<PlayerController>();
    private Dictionary<PlayerController, PlayerDoorState> playersInsideDoor = new Dictionary<PlayerController, PlayerDoorState>();

    // Helper class to store player state when they enter the door
    private class PlayerDoorState
    {
        public bool wasMovementEnabled;
        public SpriteRenderer spriteRenderer;
        public Rigidbody2D rigidbody;
        public PlayerController controller;
    }

    void Awake()
    {
        // Get or add components
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // Set initial state
        if (!requiresCheckpoint)
        {
            Debug.Log("[LevelDoor] Door does NOT require checkpoint - unlocking immediately");
            UnlockDoor();
        }
        else
        {
            Debug.Log("[LevelDoor] Door REQUIRES checkpoint - waiting for activation");
            UpdateVisuals();
        }

        // Subscribe to checkpoint events
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnCheckpointActivated += OnCheckpointActivated;
            Debug.Log("[LevelDoor] Successfully subscribed to CheckpointManager events");
        }
        else
        {
            Debug.LogError("[LevelDoor] ERROR: CheckpointManager.Instance is NULL! Add CheckpointManager to scene.");
        }

        Debug.Log($"[LevelDoor] Requires All Players: {requiresAllPlayers}");
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnCheckpointActivated -= OnCheckpointActivated;
        }

        // Re-enable any players that were inside the door
        foreach (var kvp in playersInsideDoor)
        {
            PlayerController player = kvp.Key;
            PlayerDoorState state = kvp.Value;

            if (player != null)
            {
                player.enabled = true;
                if (state.spriteRenderer != null)
                {
                    state.spriteRenderer.enabled = true;
                }
            }
        }
        playersInsideDoor.Clear();
    }

    void OnCheckpointActivated(Checkpoint checkpoint)
    {
        Debug.Log("[LevelDoor] OnCheckpointActivated event received!");

        if (requiresCheckpoint && !isUnlocked)
        {
            Debug.Log("[LevelDoor] Checkpoint requirement met - unlocking door");
            UnlockDoor();
        }
        else if (isUnlocked)
        {
            Debug.Log("[LevelDoor] Door already unlocked");
        }
        else
        {
            Debug.Log("[LevelDoor] Door does not require checkpoint");
        }
    }

    void UnlockDoor()
    {
        if (isUnlocked)
        {
            Debug.Log("[LevelDoor] UnlockDoor called but already unlocked");
            return;
        }

        isUnlocked = true;
        UpdateVisuals();

        Debug.Log("[LevelDoor] *** DOOR IS NOW UNLOCKED ***");

        // Play unlock effects
        if (unlockParticles != null)
            unlockParticles.Play();

        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }
    }

    void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isUnlocked ? unlockedColor : lockedColor;
        }
    }

    void Update()
    {
        // Check for jump input from players near the door
        foreach (PlayerController player in playersNearDoor)
        {
            if (player == null)
                continue;

            // Check if this player is already inside the door
            if (playersInsideDoor.ContainsKey(player))
            {
                // Player is inside - check if they want to exit
                if (IsPlayerPressingJump(player))
                {
                    ExitDoor(player);
                }
            }
            else if (isUnlocked && !isTransitioning)
            {
                // Player is near door but not inside - check if they want to enter
                if (IsPlayerPressingJump(player))
                {
                    EnterDoor(player);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Track when a player enters the door area
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            playersNearDoor.Add(player);
            Debug.Log($"[LevelDoor] Player {player.GetPlayerNumber()} entered door area. Door unlocked: {isUnlocked}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Remove player from tracking when they leave the door area
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            playersNearDoor.Remove(player);
        }
    }

    bool IsPlayerPressingJump(PlayerController player)
    {
        if (player == null)
            return false;

        PlayerInput playerInput = player.GetPlayerInput();
        if (playerInput == null)
            return false;

        // Get the Jump action from the player's input
        InputAction jumpAction = playerInput.actions.FindAction("Jump");
        if (jumpAction == null)
            return false;

        return jumpAction.WasPressedThisFrame();
    }

    void EnterDoor(PlayerController player)
    {
        if (player == null || playersInsideDoor.ContainsKey(player))
            return;

        Debug.Log($"[LevelDoor] Player {player.GetPlayerNumber()} entering door...");

        // Store player state
        PlayerDoorState state = new PlayerDoorState();
        state.controller = player;
        state.spriteRenderer = player.GetComponent<SpriteRenderer>();
        state.rigidbody = player.GetComponent<Rigidbody2D>();

        // Hide the player
        if (state.spriteRenderer != null)
        {
            state.spriteRenderer.enabled = false;
        }

        // Stop player movement
        if (state.rigidbody != null)
        {
            state.rigidbody.linearVelocity = Vector2.zero;
            state.rigidbody.angularVelocity = 0f;
        }

        // Disable the PlayerController so they can't move
        player.enabled = false;

        playersInsideDoor.Add(player, state);

        Debug.Log($"[LevelDoor] Player {player.GetPlayerNumber()} is now inside door (Total inside: {playersInsideDoor.Count})");

        // Play enter sound
        if (audioSource != null && doorEnterSound != null)
        {
            audioSource.PlayOneShot(doorEnterSound);
        }

        // Check if all players are now inside
        CheckIfAllPlayersInside();
    }

    void ExitDoor(PlayerController player)
    {
        if (player == null || !playersInsideDoor.ContainsKey(player))
            return;

        Debug.Log($"[LevelDoor] Player {player.GetPlayerNumber()} exiting door...");

        PlayerDoorState state = playersInsideDoor[player];

        // Re-enable the PlayerController first
        if (player != null)
        {
            player.enabled = true;
        }

        // Show the player again
        if (state != null && state.spriteRenderer != null)
        {
            state.spriteRenderer.enabled = true;
        }

        // Remove from dictionary
        playersInsideDoor.Remove(player);

        Debug.Log($"[LevelDoor] Player {player.GetPlayerNumber()} exited (Total inside: {playersInsideDoor.Count})");

        // Play exit sound
        if (audioSource != null && doorExitSound != null)
        {
            audioSource.PlayOneShot(doorExitSound);
        }
    }

    void CheckIfAllPlayersInside()
    {
        Debug.Log("[LevelDoor] CheckIfAllPlayersInside called");

        if (!requiresAllPlayers)
        {
            Debug.Log("[LevelDoor] Does NOT require all players - loading immediately");
            // If we don't require all players, load immediately when anyone enters
            LoadNextLevel();
            return;
        }

        Debug.Log("[LevelDoor] REQUIRES all players - checking player count...");

        // Get all active players
        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();
        if (spawner == null)
        {
            Debug.LogError("[LevelDoor] ERROR: PlayerSpawner not found in scene!");
            return;
        }

        var activePlayers = spawner.GetActivePlayers();
        int totalPlayers = activePlayers.Count;
        int playersInside = playersInsideDoor.Count;

        Debug.Log($"[LevelDoor] Players inside: {playersInside} / Total players: {totalPlayers}");

        if (playersInside >= totalPlayers && totalPlayers > 0)
        {
            Debug.Log("[LevelDoor] *** ALL PLAYERS ARE INSIDE! Loading next level... ***");
            LoadNextLevel();
        }
        else if (totalPlayers == 0)
        {
            Debug.LogWarning("[LevelDoor] WARNING: Total players is 0!");
        }
        else
        {
            Debug.Log($"[LevelDoor] Waiting for more players... ({playersInside}/{totalPlayers})");
        }
    }

    void LoadNextLevel()
    {
        if (isTransitioning)
        {
            Debug.Log("[LevelDoor] Already transitioning, ignoring LoadNextLevel call");
            return;
        }

        Debug.Log("[LevelDoor] ========== LOADING NEXT LEVEL ==========");
        StartCoroutine(TransitionToNextLevel());
    }

    IEnumerator TransitionToNextLevel()
    {
        isTransitioning = true;

        // Wait for transition delay
        yield return new WaitForSeconds(transitionDelay);

        // Load next scene
        if (nextSceneBuildIndex >= 0)
        {
            // Load by build index
            if (nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
            {
                Debug.Log($"Loading scene at build index {nextSceneBuildIndex}");
                SceneManager.LoadScene(nextSceneBuildIndex);
            }
            else
            {
                Debug.LogError($"Scene build index {nextSceneBuildIndex} is out of range! Total scenes: {SceneManager.sceneCountInBuildSettings}");
            }
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"Loading scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("No next scene specified! Set nextSceneName or nextSceneBuildIndex in the inspector.");
        }
    }

    // Public method to manually unlock (useful for testing)
    public void ForceUnlock()
    {
        UnlockDoor();
    }

    // Check if door is unlocked
    public bool IsUnlocked()
    {
        return isUnlocked;
    }

    // Visualize the trigger in editor
    void OnDrawGizmos()
    {
        Gizmos.color = isUnlocked ? Color.green : Color.red;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}
