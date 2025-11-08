using UnityEngine;

/// <summary>
/// Singleton manager that controls gravity state for all players in the game.
/// This is the central hub for gravity flipping - all players listen to this.
/// </summary>
public class GravityManager : MonoBehaviour
{
    // Singleton instance
    private static GravityManager instance;
    public static GravityManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GravityManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GravityManager");
                    instance = go.AddComponent<GravityManager>();
                }
            }
            return instance;
        }
    }

    [Header("Gravity Settings")]
    [Tooltip("Normal gravity scale (positive = down, negative = up)")]
    public float normalGravity = 2.5f;

    [Tooltip("Inverted gravity scale")]
    public float invertedGravity = -2.5f;

    [Header("Current State")]
    [Tooltip("Is gravity currently inverted?")]
    public bool isGravityInverted = false;

    // Event that fires when gravity is flipped
    public delegate void GravityFlipEvent(bool isInverted);
    public event GravityFlipEvent OnGravityFlip;

    private void Awake()
    {
        // Enforce singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Flips gravity for all players in the game.
    /// Call this from buttons, triggers, or any other game event.
    /// </summary>
    public void FlipGravity()
    {
        isGravityInverted = !isGravityInverted;

        Debug.Log($"Gravity flipped! Now: {(isGravityInverted ? "INVERTED" : "NORMAL")}");

        // Notify all listeners (players) about the gravity change
        OnGravityFlip?.Invoke(isGravityInverted);

        // Apply gravity to all players in the scene
        ApplyGravityToAllPlayers();
    }

    /// <summary>
    /// Sets gravity to a specific state (not a toggle).
    /// </summary>
    public void SetGravity(bool inverted)
    {
        if (isGravityInverted != inverted)
        {
            FlipGravity();
        }
    }

    /// <summary>
    /// Gets the current gravity scale value.
    /// </summary>
    public float GetCurrentGravityScale()
    {
        return isGravityInverted ? invertedGravity : normalGravity;
    }

    /// <summary>
    /// Applies the current gravity to all TigerHacksPlayerController objects in the scene.
    /// This is called automatically when gravity flips, but can be called manually.
    /// </summary>
    public void ApplyGravityToAllPlayers()
    {
        TigerHacksPlayerController[] players = FindObjectsOfType<TigerHacksPlayerController>();
        float currentGravity = GetCurrentGravityScale();

        foreach (TigerHacksPlayerController player in players)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = currentGravity;

                // Flip player sprite if inverted
                if (isGravityInverted)
                {
                    player.transform.localScale = new Vector3(
                        player.transform.localScale.x,
                        -Mathf.Abs(player.transform.localScale.y),
                        player.transform.localScale.z
                    );
                }
                else
                {
                    player.transform.localScale = new Vector3(
                        player.transform.localScale.x,
                        Mathf.Abs(player.transform.localScale.y),
                        player.transform.localScale.z
                    );
                }
            }
        }

        Debug.Log($"Applied gravity scale {currentGravity} to {players.Length} players");
    }

    /// <summary>
    /// Call this when a new player spawns to sync them with current gravity state.
    /// </summary>
    public void RegisterPlayer(TigerHacksPlayerController player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = GetCurrentGravityScale();

            // Set initial flip state
            if (isGravityInverted)
            {
                player.transform.localScale = new Vector3(
                    player.transform.localScale.x,
                    -Mathf.Abs(player.transform.localScale.y),
                    player.transform.localScale.z
                );
            }
        }
    }
}
