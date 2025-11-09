using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;
using System.Collections.Generic;



public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int maxPlayers = 4;

    [Header("Spawn Positions")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Player Colors")]
    [SerializeField]
    private List<Color> playerColors = new List<Color>()
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
        inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
        DeathSystem.OnAnyPlayerDied += HandlePlayerDeath;
    }

    void Start()
    {
        // Pre-spawn keyboard players if split keyboard is enabled
        if (allowKeyboardSplit && !useManualJoining)
        {
            Debug.Log("Key board split enabled letting in players");
            StartCoroutine(SpawnKeyboardPlayers());
        }
    }


    void Update()
    {
        // Check for players trying to join
        if (activePlayers.Count < maxPlayers)
        {
            // Player 1 - Spacebar
            if (Keyboard.current.spaceKey.wasPressedThisFrame && !HasPlayerWithScheme("Keyboard1"))
            {
                StartCoroutine(SpawnPlayerWithScheme("Keyboard1", 0));
            }
            // Player 2 - Enter
            else if (Keyboard.current.enterKey.wasPressedThisFrame && !HasPlayerWithScheme("Keyboard2"))
            {
                StartCoroutine(SpawnPlayerWithScheme("Keyboard2", 1));
            }
            // Player 3 - Numpad Enter
            else if (Keyboard.current.numpadEnterKey.wasPressedThisFrame && !HasPlayerWithScheme("Numpad"))
            {
                StartCoroutine(SpawnPlayerWithScheme("Numpad", 2));
            }
            // Player 4 - Gamepad Start
            else if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame && !HasPlayerWithScheme("Gamepad"))
            {
                StartCoroutine(SpawnPlayerWithScheme("Gamepad", 3));
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
        if (playerPrefab == null) yield break;

        Vector3 spawnPosition = (playerIndex < spawnPoints.Count && spawnPoints[playerIndex] != null)
            ? spawnPoints[playerIndex].position
            : new Vector3(playerIndex * 3f, 0f, 0f);

        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        yield return null; // Wait for initialization

        if (player == null)
        {
            Debug.LogError($"Player {playerIndex + 1} was destroyed during spawn!");
            yield break;
        }

        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null && playerInput.user.valid)
        {
            // Switch to the appropriate control scheme
            InputDevice device = (schemeName == "Gamepad") ? (InputDevice)Gamepad.current : Keyboard.current;
            playerInput.SwitchCurrentControlScheme(schemeName, device);
            playerInput.neverAutoSwitchControlSchemes = true;
        }

        string controlName = GetControlDisplayName(schemeName);
        player.name = $"Player {activePlayers.Count + 1} ({controlName})";

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetPlayerNumber(activePlayers.Count + 1);
            if (activePlayers.Count < playerColors.Count)
            {
                controller.SetPlayerColor(playerColors[activePlayers.Count]);
            }
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

                Debug.Log(spawnPoints);

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

    private void OnDisable()
    {
        DeathSystem.OnAnyPlayerDied -= HandlePlayerDeath;
    }

}