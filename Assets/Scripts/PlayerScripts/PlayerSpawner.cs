using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private int maxPlayers = 2; // Changed to 2

    [Header("Spawn Positions")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Keyboard Split Support")]
    [SerializeField] private bool allowKeyboardSplit = true;

    private List<GameObject> activePlayers = new List<GameObject>();

    void Awake()
    {
        DeathSystem.OnAnyPlayerDied += HandlePlayerDeath;
    }

    void Start()
    {
        Debug.Log("PlayerSpawner Start() running on " + gameObject.name);
        // Pre-spawn keyboard players if split keyboard is enabled
        if (allowKeyboardSplit)
        {
            Debug.Log("Keyboard split enabled, spawning players\n");
            StartCoroutine(SpawnKeyboardPlayers());
        }
            else
        {
            Debug.Log("Keyboard split disabled, waiting for manual join");
        }
    }

    void Update()
    {
        // Check for players trying to join manually
        if (activePlayers.Count < maxPlayers)
        {
            // Player 1 - Spacebar
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !HasPlayerWithScheme("Keyboard1"))
            {
                StartCoroutine(SpawnPlayerWithScheme("Keyboard1", 0));
            }
            // Player 2 - Enter
            else if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame && !HasPlayerWithScheme("Keyboard2"))
            {
                StartCoroutine(SpawnPlayerWithScheme("Keyboard2", 1));
            }
        }
    }

    public void HandlePlayerDeath()
    {
        GravityManager.Instance.SetGravity(false);
        RespawnAllPlayers();
    }

    bool HasPlayerWithScheme(string schemeName)
    {
        foreach (var player in activePlayers)
        {
            if (player == null) continue;
            var input = player.GetComponent<PlayerInput>();
            if (input != null && input.currentControlScheme == schemeName)
                return true;
        }
        return false;
    }

    IEnumerator SpawnPlayerWithScheme(string schemeName, int playerIndex)
    {
        if (playerIndex >= playerPrefabs.Length || playerPrefabs[playerIndex] == null)
        {
            Debug.LogError($"Player prefab at index {playerIndex} is null!");
            yield break;
        }

        GameObject playerPrefab = playerPrefabs[playerIndex];

        Vector3 spawnPosition = (playerIndex < spawnPoints.Count && spawnPoints[playerIndex] != null)
            ? spawnPoints[playerIndex].position
            : new Vector3(playerIndex * 3f, 0f, 0f);

        // Instantiate the player
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        
        // IMPORTANT: Don't parent to this GameObject - keep it in root
        player.transform.parent = null;

        yield return new WaitForEndOfFrame(); // Wait for full initialization

        if (player == null)
        {
            Debug.LogError($"Player {playerIndex + 1} was destroyed during spawn!");
            yield break;
        }

        // Configure PlayerInput
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            // Disable PlayerInput temporarily to prevent auto-switching
            playerInput.DeactivateInput();
            
            // Switch to the appropriate control scheme
            InputDevice device = (schemeName == "Gamepad" && Gamepad.current != null) 
                ? (InputDevice)Gamepad.current 
                : Keyboard.current;
            
            if (device != null)
            {
                playerInput.SwitchCurrentControlScheme(schemeName, device);
            }
            
            playerInput.neverAutoSwitchControlSchemes = true;
            
            // Re-enable input
            playerInput.ActivateInput();
        }
        else
        {
            Debug.LogWarning($"Player {playerIndex + 1} missing PlayerInput component!");
        }

        string controlName = GetControlDisplayName(schemeName);
        player.name = $"Player {activePlayers.Count + 1} ({controlName})";

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetPlayerNumber(activePlayers.Count + 1);
        }

        activePlayers.Add(player);
        Debug.Log($"Player {activePlayers.Count} joined with {schemeName}");
    }

    string GetControlDisplayName(string controlScheme)
    {
        switch (controlScheme)
        {
            case "Keyboard1": return "WASD";
            case "Keyboard2": return "Arrow Keys";
            case "Numpad": return "Numpad";
            case "Gamepad": return "Controller";
            default: return controlScheme;
        }
    }

    IEnumerator SpawnKeyboardPlayers()
    {
        // Wait for scene to fully initialize
        yield return new WaitForEndOfFrame();

        // Spawn only 2 keyboard players
        for (int i = 0; i < Mathf.Min(2, maxPlayers, playerPrefabs.Length); i++)
        {
            if (playerPrefabs[i] != null)
            {
                SpawnKeyboardPlayer(i);
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                Debug.LogError($"Player prefab at index {i} is null!");
            }
        }
    }

    void SpawnKeyboardPlayer(int index)
    {
        if (index >= playerPrefabs.Length || playerPrefabs[index] == null)
        {
            Debug.LogError($"Cannot spawn player {index}: Prefab is null!");
            return;
        }

        GameObject playerPrefab = playerPrefabs[index];

        // Determine spawn position
        Vector3 spawnPosition = (index < spawnPoints.Count && spawnPoints[index] != null)
            ? spawnPoints[index].position
            : new Vector3(index * 3f, 0f, 0f);

        // Instantiate directly
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        
        // Keep in root hierarchy
        player.transform.parent = null;

        // Determine control scheme and name
        string schemeName = index == 0 ? "Keyboard1" : "Keyboard2";
        string controlName = index == 0 ? "WASD" : "Arrow Keys";

        player.name = $"Player {index + 1} ({controlName})";

        // Configure PlayerInput
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null && Keyboard.current != null)
        {
            playerInput.DeactivateInput();
            playerInput.SwitchCurrentControlScheme(schemeName, Keyboard.current);
            playerInput.neverAutoSwitchControlSchemes = true;
            playerInput.ActivateInput();
        }
        else if (playerInput == null)
        {
            Debug.LogWarning($"Player {index + 1} missing PlayerInput component!");
        }

        // Configure PlayerController
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetPlayerNumber(index + 1);
        }

        activePlayers.Add(player);
        Debug.Log($"Keyboard Player {index + 1} spawned with {controlName}");
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

    public void RespawnAllPlayers()
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            RespawnPlayer(i);
        }
    }

    public GameObject GetPlayer(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < activePlayers.Count)
        {
            return activePlayers[playerIndex];
        }
        return null;
    }

    public List<GameObject> GetActivePlayers()
    {
        return new List<GameObject>(activePlayers);
    }

    public int GetPlayerCount()
    {
        return activePlayers.Count;
    }

    public void RemovePlayer(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < activePlayers.Count)
        {
            GameObject player = activePlayers[playerIndex];
            activePlayers.RemoveAt(playerIndex);
            Destroy(player);
        }
    }

    private void OnDisable()
    {
        DeathSystem.OnAnyPlayerDied -= HandlePlayerDeath;
    }
}