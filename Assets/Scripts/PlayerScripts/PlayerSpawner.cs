using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
// using System.Diagnostics;

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
    
    [Header("Keyboard Split Support")]
    [SerializeField] private bool allowKeyboardSplit = true;
    [SerializeField] private int maxKeyboardPlayers = 3; // WASD, Arrow Keys, Numpad
    
    [Header("Manual Join Settings")]
    [SerializeField] private bool useManualJoining = false;
    [SerializeField] private KeyCode player1JoinKey = KeyCode.Return;
    [SerializeField] private KeyCode player2JoinKey = KeyCode.KeypadEnter;
    
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
        inputManager.joinBehavior = useManualJoining 
            ? PlayerJoinBehavior.JoinPlayersManually 
            : PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
        
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
        // Pre-spawn keyboard players if split keyboard is enabled
        if (allowKeyboardSplit && !useManualJoining)
        {
            Debug.Log("Key board split enabled letting in players");
            StartCoroutine(SpawnKeyboardPlayers());
        }
        
        if (useManualJoining)
        {
            Debug.Log("Manual joining enabled. Press assigned keys to join players.");
        }
        else if (allowKeyboardSplit)
        {
            Debug.Log("Keyboard split enabled. Keyboard players will spawn automatically. Gamepad players can join by pressing any button.");
        }
        else
        {
            Debug.Log("PlayerSpawner ready. Players can join by pressing any button on their device.");
        }
    }
    
    System.Collections.IEnumerator SpawnKeyboardPlayers()
    {
        // Wait a frame for everything to initialize
        yield return null;
        
        // Spawn keyboard players directly without using PlayerInputManager
        for (int i = 0; i < Mathf.Min(maxKeyboardPlayers, maxPlayers); i++)
        {
            SpawnKeyboardPlayer(i);
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    void SpawnKeyboardPlayer(int index)
    {
        if (index >= maxPlayers || playerPrefab == null)
            return;
        
        // Determine spawn position
        Vector3 spawnPosition;
        if (index < spawnPoints.Count && spawnPoints[index] != null)
        {
            spawnPosition = spawnPoints[index].position;
        }
        else
        {
            spawnPosition = new Vector3(index * 3f, 0f, 0f);
        }
        
        // Instantiate directly (bypass PlayerInputManager)
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        
        // Determine control scheme and name
        string schemeName = "Keyboard1";
        string controlName = "WASD";
        if (index == 1)
        {
            schemeName = "Keyboard2";
            controlName = "Arrow Keys";
        }
        else if (index == 2)
        {
            schemeName = "Numpad";
            controlName = "Numpad 8246";
        }
        
        player.name = $"Player {index + 1} ({controlName})";
        
        // Configure PlayerInput to use specific control scheme
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.SwitchCurrentControlScheme(schemeName, UnityEngine.InputSystem.Keyboard.current);
            playerInput.neverAutoSwitchControlSchemes = true; // Lock to this scheme
        }
        
        // Configure PlayerController
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetPlayerNumber(index + 1);
            
            if (index < playerColors.Count)
            {
                controller.SetPlayerColor(playerColors[index]);
            }
        }
        
        activePlayers.Add(player);
        Debug.Log($"Keyboard Player {index + 1} spawned with {controlName}");
    }
    
    void Update()
    {
        if (useManualJoining)
        {
            HandleManualJoining();
        }
    }
    
    void HandleManualJoining()
    {
        if (activePlayers.Count >= maxPlayers)
            return;
        
        // Check for player join keys
        if (Input.GetKeyDown(player1JoinKey) && !IsKeyboardPlayerActive("Keyboard1"))
        {
            JoinPlayerWithScheme(0); // Keyboard1 scheme index
        }
        
        if (Input.GetKeyDown(player2JoinKey) && !IsKeyboardPlayerActive("Keyboard2"))
        {
            JoinPlayerWithScheme(1); // Keyboard2 scheme index
        }
        
        // Gamepads join by pressing any button (automatic)
    }
    
    bool IsKeyboardPlayerActive(string schemeName)
    {
        foreach (GameObject player in activePlayers)
        {
            PlayerInput pi = player.GetComponent<PlayerInput>();
            if (pi != null && pi.currentControlScheme == schemeName)
                return true;
        }
        return false;
    }
    
    void JoinPlayerWithScheme(int controlSchemeIndex)
    {
        if (inputManager != null)
        {
            PlayerInput.Instantiate(playerPrefab, controlSchemeIndex, 
                pairWithDevice: UnityEngine.InputSystem.Keyboard.current);
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
            inputManager.joinBehavior = enabled 
                ? PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed 
                : PlayerJoinBehavior.JoinPlayersManually;
        }
    }
}