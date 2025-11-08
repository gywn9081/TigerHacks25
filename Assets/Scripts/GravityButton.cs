using UnityEngine;

/// <summary>
/// A button that flips gravity when a player touches it.
/// Attach this to any GameObject with a 2D collider (set to Trigger).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GravityButton : MonoBehaviour
{
    [Header("Button Settings")]
    [Tooltip("Can the button be pressed multiple times?")]
    public bool isReusable = true;

    [Tooltip("Cooldown between presses (seconds). Only used if isReusable = true")]
    public float cooldownTime = 1f;

    [Header("Visual Feedback")]
    [Tooltip("Sprite for unpressed state")]
    public Sprite unpressedSprite;

    [Tooltip("Sprite for pressed state")]
    public Sprite pressedSprite;

    [Tooltip("Color when unpressed")]
    public Color unpressedColor = Color.white;

    [Tooltip("Color when pressed")]
    public Color pressedColor = Color.green;

    [Header("Audio (Optional)")]
    [Tooltip("Sound to play when button is pressed")]
    public AudioClip pressSound;

    // Internal state
    private bool isPressed = false;
    private float lastPressTime = -999f;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Ensure collider is set to trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Set initial visual state
        UpdateVisuals();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if a player touched the button
        if (other.GetComponent<PlayerController>() != null)
        {
            PressButton();
        }
    }

    /// <summary>
    /// Presses the button and flips gravity.
    /// Can be called manually from code or triggers.
    /// </summary>
    public void PressButton()
    {
        // Check if button can be pressed
        if (!isReusable && isPressed)
        {
            Debug.Log($"{gameObject.name} - Button already pressed and is not reusable");
            return;
        }

        if (isReusable && Time.time < lastPressTime + cooldownTime)
        {
            Debug.Log($"{gameObject.name} - Button on cooldown");
            return;
        }

        // Press the button
        isPressed = true;
        lastPressTime = Time.time;

        Debug.Log($"{gameObject.name} - Button pressed! Flipping gravity...");

        // Flip gravity globally
        GravityManager.Instance.FlipGravity();

        // Update visuals
        UpdateVisuals();

        // Play sound if available
        if (audioSource != null && pressSound != null)
        {
            audioSource.PlayOneShot(pressSound);
        }

        // If reusable, schedule unpressing
        if (isReusable)
        {
            Invoke(nameof(UnpressButton), 0.2f); // Visual feedback duration
        }
    }

    private void UnpressButton()
    {
        // Don't actually change state, just update visuals to show it can be pressed again
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer == null)
            return;

        // Check if we're in cooldown
        bool inCooldown = isReusable && Time.time < lastPressTime + cooldownTime;

        // Update sprite
        if (pressedSprite != null && unpressedSprite != null)
        {
            spriteRenderer.sprite = inCooldown ? pressedSprite : unpressedSprite;
        }

        // Update color
        spriteRenderer.color = inCooldown ? pressedColor : unpressedColor;
    }

    private void Update()
    {
        // Update visuals each frame (for cooldown transitions)
        if (isReusable)
        {
            UpdateVisuals();
        }
    }

    // Visualize trigger area in editor
    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = isPressed ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}
