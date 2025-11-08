using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance { get; private set; }
    
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
    
    [Header("Input Settings")]
    [SerializeField] private bool allowKeyboardSplit = false; // Allow multiple keyboard players
    
    private PlayerInputManager playerInputManager;
    private List<GameObject> activePlayers = new List<GameObject>();
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Get or add PlayerInputManager component
        playerInputManager = GetComponent<PlayerInputManager>();
        if (playerInputManager == null)
        {
            playerInputManager = gameObject.AddComponent<PlayerInputManager>();
        }
    }
    
    // void Start()
    // {
    //     // Enable joining for all players
    //     if (playerInputManager != null)
    //     {
    //         playerInputManager.EnableJoining();
    //     }
    // }
    
    // Called automatically by PlayerInputManager when a player joins
    public void OnPlayerJoined(PlayerInput playerInput)
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
        
        activePlayers.Add(playerInput.gameObject);
        
        Debug.Log($"Player {playerIndex + 1} joined with {playerInput.currentControlScheme}");
    }
    
    // Called automatically when a player leaves
    public void OnPlayerLeft(PlayerInput playerInput)
    {
        activePlayers.Remove(playerInput.gameObject);
        Debug.Log($"Player left. Active players: {activePlayers.Count}");
    }
    
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
                
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }
    }
    
    public List<GameObject> GetActivePlayers()
    {
        return activePlayers;
    }
    
    public int GetPlayerCount()
    {
        return activePlayers.Count;
    }
}