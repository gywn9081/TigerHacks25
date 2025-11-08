using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool requiresAllPlayers = true;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem activationParticles;
    [SerializeField] private AudioClip activationSound;

    private bool isActivated = false;
    private AudioSource audioSource;

    void Awake()
    {
        // Get or add components
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && activationSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // Set initial color
        if (spriteRenderer != null)
            spriteRenderer.color = inactiveColor;

        // Subscribe to player death events
        PlayerSpawner.OnAnyPlayerDied += OnPlayerDeath;
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        PlayerSpawner.OnAnyPlayerDied -= OnPlayerDeath;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Checkpoint] ===== TRIGGER ENTER ===== Object: {other.gameObject.name}");

        // Check if a player touched the checkpoint
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
        {
            Debug.Log($"[Checkpoint] Object '{other.gameObject.name}' does NOT have PlayerController");
            return;
        }

        Debug.Log($"[Checkpoint] Player {player.GetPlayerNumber()} touched checkpoint! isActivated={isActivated}");

        if (isActivated)
        {
            Debug.Log($"[Checkpoint] Checkpoint already activated, ignoring");
            return;
        }

        if (requiresAllPlayers)
        {
            Debug.Log("[Checkpoint] Requires ALL players - checking if all present...");
            // Check if all players are touching the checkpoint
            CheckAllPlayersTouching();
        }
        else
        {
            Debug.Log("[Checkpoint] Does NOT require all players - activating immediately");
            // Activate immediately when any player touches it
            ActivateCheckpoint();
        }
    }

    void CheckAllPlayersTouching()
    {
        // Get all active players
        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("[Checkpoint] PlayerSpawner not found in scene!");
            return;
        }

        var activePlayers = spawner.GetActivePlayers();
        if (activePlayers.Count == 0)
        {
            Debug.LogWarning("[Checkpoint] No active players found!");
            return;
        }

        Debug.Log($"[Checkpoint] Total active players: {activePlayers.Count}");

        // Check if all players are within the checkpoint trigger
        int playersInCheckpoint = 0;
        Collider2D checkpointCollider = GetComponent<Collider2D>();

        foreach (GameObject playerObj in activePlayers)
        {
            if (playerObj != null)
            {
                Collider2D playerCollider = playerObj.GetComponent<Collider2D>();
                if (playerCollider != null && checkpointCollider.IsTouching(playerCollider))
                {
                    playersInCheckpoint++;
                    PlayerController pc = playerObj.GetComponent<PlayerController>();
                    if (pc != null)
                    {
                        Debug.Log($"[Checkpoint] Player {pc.GetPlayerNumber()} is touching checkpoint");
                    }
                }
            }
        }

        Debug.Log($"[Checkpoint] Players touching checkpoint: {playersInCheckpoint}/{activePlayers.Count}");

        // Activate if all players are present
        if (playersInCheckpoint >= activePlayers.Count)
        {
            Debug.Log("[Checkpoint] *** ALL PLAYERS PRESENT - ACTIVATING ***");
            ActivateCheckpoint();
        }
        else
        {
            Debug.Log($"[Checkpoint] Waiting for more players ({playersInCheckpoint}/{activePlayers.Count})");
        }
    }

    void ActivateCheckpoint()
    {
        if (isActivated)
        {
            Debug.Log("[Checkpoint] Already activated, ignoring");
            return;
        }

        isActivated = true;

        Debug.Log("[Checkpoint] *** CHECKPOINT ACTIVATED ***");

        // Visual feedback
        if (spriteRenderer != null)
            spriteRenderer.color = activeColor;

        // Particle effect
        if (activationParticles != null)
            activationParticles.Play();

        // Sound effect
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        // Notify the checkpoint manager
        if (CheckpointManager.Instance != null)
        {
            Debug.Log("[Checkpoint] Notifying CheckpointManager");
            CheckpointManager.Instance.ActivateCheckpoint(this);
        }
        else
        {
            Debug.LogError("[Checkpoint] ERROR: CheckpointManager.Instance is NULL!");
        }
    }

    public bool IsActivated()
    {
        return isActivated;
    }

    // Public method to manually activate (useful for testing)
    public void ForceActivate()
    {
        ActivateCheckpoint();
    }

    // Called when a player dies - reset the checkpoint state
    void OnPlayerDeath()
    {
        Debug.Log("[Checkpoint] Player died - resetting checkpoint state");
        ResetCheckpoint();
    }

    // Reset the checkpoint (useful when restarting level)
    public void ResetCheckpoint()
    {
        isActivated = false;
        if (spriteRenderer != null)
            spriteRenderer.color = inactiveColor;

        Debug.Log("[Checkpoint] Checkpoint reset to inactive state");
    }
}
