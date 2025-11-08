using UnityEngine;
using System;

public class CheckpointManager : MonoBehaviour
{
    // Singleton instance
    public static CheckpointManager Instance { get; private set; }

    // Event that fires when any checkpoint is activated
    public event Action<Checkpoint> OnCheckpointActivated;

    // Track the currently active checkpoint
    private Checkpoint activeCheckpoint;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Optional: Make this persist across scenes
        // DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Subscribe to player death events
        PlayerSpawner.OnAnyPlayerDied += OnPlayerDeath;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Unsubscribe from events
        PlayerSpawner.OnAnyPlayerDied -= OnPlayerDeath;
    }

    public void ActivateCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null)
            return;

        activeCheckpoint = checkpoint;

        Debug.Log($"[CheckpointManager] Checkpoint activated, notifying listeners...");
        Debug.Log($"[CheckpointManager] Number of listeners: {(OnCheckpointActivated != null ? OnCheckpointActivated.GetInvocationList().Length : 0)}");

        // Fire the event to notify listeners (like doors)
        OnCheckpointActivated?.Invoke(checkpoint);

        Debug.Log("[CheckpointManager] Event invoked");
    }

    public Checkpoint GetActiveCheckpoint()
    {
        return activeCheckpoint;
    }

    public bool HasActiveCheckpoint()
    {
        return activeCheckpoint != null && activeCheckpoint.IsActivated();
    }

    // Called when a player dies - reset everything
    void OnPlayerDeath()
    {
        Debug.Log("[CheckpointManager] Player died - resetting all checkpoints and doors");
        ResetAllCheckpoints();
        ResetAllDoors();
    }

    // Reset all checkpoints (useful when restarting a level)
    public void ResetAllCheckpoints()
    {
        activeCheckpoint = null;
        Checkpoint[] allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        foreach (Checkpoint checkpoint in allCheckpoints)
        {
            checkpoint.ResetCheckpoint();
        }
    }

    // Reset all doors in the scene
    public void ResetAllDoors()
    {
        LevelDoor[] allDoors = FindObjectsByType<LevelDoor>(FindObjectsSortMode.None);
        foreach (LevelDoor door in allDoors)
        {
            door.ResetDoor();
        }
    }
}
