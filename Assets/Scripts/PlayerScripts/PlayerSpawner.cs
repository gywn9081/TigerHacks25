using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int maxPlayers = 4;
    
    [Header("Spawn Positions")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    
    [Header("Player Colors")]
    [SerializeField] private List<Color> playerColors = new List<Color>()
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };
    
    private UnityEngine.InputSystem.PlayerInputManager inputManager;
    private List<GameObject> activePlayers = new List<GameObject>();
    
    void Awake()
    {
        // Get or add Unity's PlayerInputManager component
        inputManager = GetComponent<UnityEngine.InputSystem.PlayerInputManager>();
        if (inputManager == null)
        {
            inputManager = gameObject.AddComponent<UnityEngine.InputSystem.PlayerInputManager>();
        }
        
        // Configure the PlayerInputManager
        inputManager.playerPrefab = playerPrefab;
        inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
        
        // Note: maxPlayerCount must be set in the Inspector on the PlayerInputManager component
        
        // Subscribe to join/leave events
        inputManager.onPlayerJoined += OnPlayerJoined;
        inputManager.onPlayerLeft += OnPlayerLeft;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (inputManager != null)
        {
            inputManager.onPlayerJoined -= OnPlayerJoined;
            inputManager.onPlayerLeft -= OnPlayerLeft;
        }
    }
    
    void Start()
    {
        // Enable joining for all players
        if (inputManager != null)
        {
            inputManager.EnableJoining();
        }
    }
    
    // Called automatically by PlayerInputManager when a player joins
    void OnPlayerJoined(PlayerInput playerInput)
    {
        int playerIndex = activePlayers.Count;
        
        if (playerIndex >= maxPlayers)
        {
            Debug.LogWarning("Maximum number of players reached!");
            Destroy(playerInput.gameObject);
            return;
        }
        
        // Set spawn position
        Vector3 spawnPosition;
        if (playerIndex < spawnPoints.Count && spawnPoints[playerIndex] != null)
        {
            spawnPosition = spawnPoints[playerIndex].position;
        }
        else
        {
            // Default spacing if no spawn point is set
            spawnPosition = new Vector3(playerIndex * 3f, 0f, 0f);
        }
        
        playerInput.transform.position = spawnPosition;
        playerInput.gameObject.name = $"Player {playerIndex + 1}";
        
        // Configure player
        PlayerController controller = playerInput.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetPlayerNumber(playerIndex + 1);
            
            // Set player color
            if (playerIndex < playerColors.Count)
            {
                controller.SetPlayerColor(playerColors[playerIndex]);
            }
        }
        else
        {
            Debug.LogWarning($"PlayerController not found on {playerInput.gameObject.name}");
        }
        
        activePlayers.Add(playerInput.gameObject);
        
        Debug.Log($"Player {playerIndex + 1} joined using {playerInput.currentControlScheme} (Total players: {activePlayers.Count})");
    }
    
    // Called automatically when a player leaves
    void OnPlayerLeft(PlayerInput playerInput)
    {
        if (activePlayers.Contains(playerInput.gameObject))
        {
            activePlayers.Remove(playerInput.gameObject);
            Debug.Log($"Player left. Active players: {activePlayers.Count}");
        }
    }
    
    // Manual player spawning (if you want to spawn players programmatically)
    public GameObject SpawnPlayer(int controlSchemeIndex = -1)
    {
        if (activePlayers.Count >= maxPlayers)
        {
            Debug.LogWarning("Cannot spawn player: Maximum player count reached!");
            return null;
        }
        
        if (inputManager != null)
        {
            // Use PlayerInputManager to spawn
            PlayerInput newPlayer = inputManager.JoinPlayer(controlSchemeIndex);
            return newPlayer != null ? newPlayer.gameObject : null;
        }
        
        return null;
    }
    
    // Respawn a player at their spawn point
    public void RespawnPlayer(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < activePlayers.Count)
        {
            GameObject player = activePlayers[playerIndex];
            
            if (player != null)
            {
                Vector3 spawnPosition = (playerIndex < spawnPoints.Count && spawnPoints[playerIndex] != null)
                    ? spawnPoints[playerIndex].position
                    : new Vector3(playerIndex * 3f, 0f, 0f);
                
                player.transform.position = spawnPosition;
                
                // Reset physics
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
            }
        }
        else
        {
            Debug.LogWarning($"Cannot respawn player {playerIndex}: Invalid index");
        }
    }
    
    // Respawn all players
    public void RespawnAllPlayers()
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            RespawnPlayer(i);
        }
    }
    
    // Get a specific player
    public GameObject GetPlayer(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < activePlayers.Count)
        {
            return activePlayers[playerIndex];
        }
        return null;
    }
    
    // Get all active players
    public List<GameObject> GetActivePlayers()
    {
        return new List<GameObject>(activePlayers);
    }
    
    // Get current player count
    public int GetPlayerCount()
    {
        return activePlayers.Count;
    }
    
    // Remove a specific player
    public void RemovePlayer(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < activePlayers.Count)
        {
            GameObject player = activePlayers[playerIndex];
            activePlayers.RemoveAt(playerIndex);
            Destroy(player);
        }
    }
    
    // Enable/disable player joining
    public void SetJoiningEnabled(bool enabled)
    {
        if (inputManager != null)
        {
            if (enabled)
                inputManager.EnableJoining();
            else
                inputManager.DisableJoining();
        }
    }
}