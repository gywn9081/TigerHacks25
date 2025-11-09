using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RocketLaunch : MonoBehaviour
{
    [Header("Scene Settings")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
#endif
    [SerializeField, HideInInspector] private string nextSceneName;

    [Header("Rocket Settings")]
    [SerializeField] private bool requiresCheckpoint = true;
    [SerializeField] private bool requiresAllPlayers = true;
    [SerializeField] private Transform rocketTransform; // The rocket sprite
    [SerializeField] private float launchSpeed = 10f;
    [SerializeField] private float launchDuration = 2f;

    [Header("Flame Animation")]
    [SerializeField] private RocketFlameController flameController;

    [Header("Visual Feedback")]
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;
    [SerializeField] private SpriteRenderer spriteRenderer; // Optional indicator sprite
    [SerializeField] private ParticleSystem unlockParticles;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip launchSound;
    [SerializeField] private AudioClip boardSound;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDelay = 1.0f;

    private bool isUnlocked = false;
    private bool isLaunching = false;
    private AudioSource audioSource;
    private Vector3 initialRocketPosition;

    // Track players near the rocket and inside the rocket
    private HashSet<PlayerController> playersNearRocket = new HashSet<PlayerController>();
    private Dictionary<PlayerController, PlayerRocketState> playersInsideRocket = new Dictionary<PlayerController, PlayerRocketState>();

    // Helper class to store player state when they board the rocket
    private class PlayerRocketState
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

        // Store initial rocket position
        if (rocketTransform != null)
        {
            initialRocketPosition = rocketTransform.position;
        }
    }

    void Start()
    {
        // Set initial state
        if (!requiresCheckpoint)
        {
            Debug.Log("[RocketLaunch] Rocket does NOT require checkpoint - unlocking immediately");
            UnlockRocket();
        }
        else
        {
            Debug.Log("[RocketLaunch] Rocket REQUIRES checkpoint - waiting for activation");
            UpdateVisuals();
        }

        // Subscribe to checkpoint events
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnCheckpointActivated += OnCheckpointActivated;
            Debug.Log("[RocketLaunch] Successfully subscribed to CheckpointManager events");
        }
        else
        {
            Debug.LogError("[RocketLaunch] ERROR: CheckpointManager.Instance is NULL! Add CheckpointManager to scene.");
        }

        // Subscribe to player death events
        DeathSystem.OnAnyPlayerDied += OnPlayerDeath;

        Debug.Log($"[RocketLaunch] Requires All Players: {requiresAllPlayers}");
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnCheckpointActivated -= OnCheckpointActivated;
        }

        DeathSystem.OnAnyPlayerDied -= OnPlayerDeath;

        // Re-enable any players that were inside the rocket
        foreach (var kvp in playersInsideRocket)
        {
            PlayerController player = kvp.Key;
            PlayerRocketState state = kvp.Value;

            if (player != null)
            {
                player.enabled = true;
                if (state.spriteRenderer != null)
                {
                    state.spriteRenderer.enabled = true;
                }
            }
        }
        playersInsideRocket.Clear();
    }

    void OnCheckpointActivated(Checkpoint checkpoint)
    {
        Debug.Log("[RocketLaunch] OnCheckpointActivated event received!");

        if (requiresCheckpoint && !isUnlocked)
        {
            Debug.Log("[RocketLaunch] Checkpoint requirement met - unlocking rocket");
            UnlockRocket();
        }
        else if (isUnlocked)
        {
            Debug.Log("[RocketLaunch] Rocket already unlocked");
        }
        else
        {
            Debug.Log("[RocketLaunch] Rocket does not require checkpoint");
        }
    }

    void UnlockRocket()
    {
        Debug.Log($"[RocketLaunch] UnlockRocket() called on {gameObject.name}");

        if (isUnlocked)
        {
            Debug.Log("[RocketLaunch] UnlockRocket called but already unlocked");
            return;
        }

        isUnlocked = true;
        UpdateVisuals();

        Debug.Log("[RocketLaunch] *** ROCKET IS NOW UNLOCKED ***");

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
        // Check for jump input from players near the rocket
        foreach (PlayerController player in playersNearRocket)
        {
            if (player == null)
                continue;

            // Only allow boarding if unlocked and not already launching
            if (isUnlocked && !isLaunching && !playersInsideRocket.ContainsKey(player))
            {
                if (IsPlayerPressingJump(player))
                {
                    BoardRocket(player);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Track when a player enters the rocket area
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            playersNearRocket.Add(player);
            Debug.Log($"[RocketLaunch] Player {player.GetPlayerNumber()} entered rocket area. Rocket unlocked: {isUnlocked}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Remove player from tracking when they leave the rocket area
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            playersNearRocket.Remove(player);
        }
    }

    bool IsPlayerPressingJump(PlayerController player)
    {
        if (player == null)
            return false;

        UnityEngine.InputSystem.PlayerInput playerInput = player.GetPlayerInput();
        if (playerInput == null)
            return false;

        // Get the Jump action from the player's input
        InputAction jumpAction = playerInput.actions.FindAction("Jump");
        if (jumpAction == null)
            return false;

        return jumpAction.WasPressedThisFrame();
    }

    void BoardRocket(PlayerController player)
    {
        if (player == null || playersInsideRocket.ContainsKey(player))
            return;

        Debug.Log($"[RocketLaunch] Player {player.GetPlayerNumber()} boarding rocket...");

        // Store player state
        PlayerRocketState state = new PlayerRocketState();
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

        playersInsideRocket.Add(player, state);

        Debug.Log($"[RocketLaunch] Player {player.GetPlayerNumber()} is now inside rocket (Total inside: {playersInsideRocket.Count})");

        // Play board sound
        if (audioSource != null && boardSound != null)
        {
            audioSource.PlayOneShot(boardSound);
        }

        // Check if all players are now inside
        CheckIfAllPlayersInside();
    }

    void CheckIfAllPlayersInside()
    {
        Debug.Log("[RocketLaunch] CheckIfAllPlayersInside called");

        if (!requiresAllPlayers)
        {
            Debug.Log("[RocketLaunch] Does NOT require all players - launching immediately");
            // If we don't require all players, launch immediately when anyone boards
            LaunchRocket();
            return;
        }

        Debug.Log("[RocketLaunch] REQUIRES all players - checking player count...");

        // Get all active players
        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();
        if (spawner == null)
        {
            Debug.LogError("[RocketLaunch] ERROR: PlayerSpawner not found in scene!");
            return;
        }

        var activePlayers = spawner.GetActivePlayers();
        int totalPlayers = activePlayers.Count;
        int playersInside = playersInsideRocket.Count;

        Debug.Log($"[RocketLaunch] Players inside: {playersInside} / Total players: {totalPlayers}");

        if (playersInside >= totalPlayers && totalPlayers > 0)
        {
            Debug.Log("[RocketLaunch] *** ALL PLAYERS ARE INSIDE! Launching rocket... ***");
            LaunchRocket();
        }
        else if (totalPlayers == 0)
        {
            Debug.LogWarning("[RocketLaunch] WARNING: Total players is 0!");
        }
        else
        {
            Debug.Log($"[RocketLaunch] Waiting for more players... ({playersInside}/{totalPlayers})");
        }
    }

    void LaunchRocket()
    {
        if (isLaunching)
        {
            Debug.Log("[RocketLaunch] Already launching, ignoring LaunchRocket call");
            return;
        }

        Debug.Log("[RocketLaunch] ========== LAUNCHING ROCKET ==========");
        StartCoroutine(LaunchSequence());
    }

    IEnumerator LaunchSequence()
    {
        isLaunching = true;

        // Start the flame animation
        if (flameController != null)
        {
            Debug.Log("[RocketLaunch] Starting flame animation");
            flameController.RestartAnimation();
        }

        // Play launch sound
        if (audioSource != null && launchSound != null)
        {
            audioSource.PlayOneShot(launchSound);
        }

        // Move the rocket up
        if (rocketTransform != null)
        {
            Debug.Log("[RocketLaunch] Moving rocket upward");
            float elapsed = 0f;
            Vector3 startPosition = rocketTransform.position;

            while (elapsed < launchDuration)
            {
                elapsed += Time.deltaTime;
                rocketTransform.position += Vector3.up * launchSpeed * Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("[RocketLaunch] No rocket transform assigned, waiting for launch duration");
            yield return new WaitForSeconds(launchDuration);
        }

        // Wait for transition delay
        yield return new WaitForSeconds(transitionDelay);

        // Load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"Loading scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("No next scene specified! Set nextSceneName in the inspector.");
        }
    }

    // Public method to manually unlock (useful for testing)
    public void ForceUnlock()
    {
        UnlockRocket();
    }

    // Check if rocket is unlocked
    public bool IsUnlocked()
    {
        return isUnlocked;
    }

    // Called when a player dies - reset the rocket state
    void OnPlayerDeath()
    {
        Debug.Log("[RocketLaunch] Player died - resetting rocket state");
        ResetRocket();
    }

    // Reset the rocket to its initial locked state
    public void ResetRocket()
    {
        // Release all players from inside the rocket
        foreach (var kvp in playersInsideRocket)
        {
            PlayerController player = kvp.Key;
            PlayerRocketState state = kvp.Value;

            if (player != null)
            {
                player.enabled = true;
                if (state.spriteRenderer != null)
                {
                    state.spriteRenderer.enabled = true;
                }
            }
        }
        playersInsideRocket.Clear();
        playersNearRocket.Clear();

        // Reset rocket position
        if (rocketTransform != null)
        {
            rocketTransform.position = initialRocketPosition;
        }

        // Reset flame animation
        if (flameController != null)
        {
            flameController.StopAnimation();
        }

        // Lock the rocket if it requires a checkpoint
        if (requiresCheckpoint)
        {
            isUnlocked = false;
            UpdateVisuals();
            Debug.Log("[RocketLaunch] Rocket locked and reset");
        }

        // Cancel any ongoing launch
        StopAllCoroutines();
        isLaunching = false;
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneAsset != null)
            nextSceneName = sceneAsset.name;
    }
#endif
}
