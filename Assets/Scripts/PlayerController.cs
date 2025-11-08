using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Input Settings")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Debug")]
    public bool showDebugInfo = false;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float horizontalInput;
    private float lastJumpTime = -1f;
    private float jumpCooldown = 0.05f; // Very small cooldown to prevent double jumps

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Debug log to check layer mask setup
        string myLayerName = LayerMask.LayerToName(gameObject.layer);
        Debug.Log($"{gameObject.name} - Layer: {gameObject.layer} ({myLayerName}), Ground Layer Mask: {groundLayer.value}");

        if (groundLayer.value == 0)
        {
            Debug.LogError($"{gameObject.name} - Ground layer mask is 0! Make sure 'Ground' and 'Player' layers exist in Tags and Layers.");
        }

        // List what layers we're checking for
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        Debug.Log($"{gameObject.name} - Checking for Ground (Layer {groundLayerIndex}) and Player (Layer {playerLayerIndex})");

        // Register with GravityManager to sync with current gravity state
        if (GravityManager.Instance != null)
        {
            GravityManager.Instance.RegisterPlayer(this);
        }
    }

    private void Update()
    {
        // Get input
        HandleInput();

        // Jump - only if grounded AND cooldown has passed
        if (Input.GetKeyDown(jumpKey) && isGrounded && Time.time > lastJumpTime + jumpCooldown)
        {
            Jump();
            lastJumpTime = Time.time;
        }
    }

    private void FixedUpdate()
    {
        // Apply horizontal movement
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Check ground in FixedUpdate for consistent physics timing
        CheckGround();
    }

    private void HandleInput()
    {
        horizontalInput = 0f;

        if (Input.GetKey(leftKey))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(rightKey))
        {
            horizontalInput = 1f;
        }
    }

    private void Jump()
    {
        // Check if gravity is inverted and adjust jump direction
        float jumpDirection = 1f;
        if (GravityManager.Instance != null && GravityManager.Instance.isGravityInverted)
        {
            jumpDirection = -1f; // Jump downward when gravity is inverted
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * jumpDirection);
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            // If no ground check transform, just check if velocity is near zero
            isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.01f;
            return;
        }

        isGrounded = false;

        // Determine direction based on gravity
        bool gravityInverted = GravityManager.Instance != null && GravityManager.Instance.isGravityInverted;
        Vector2 checkDirection = gravityInverted ? Vector2.up : Vector2.down;
        float directionMultiplier = gravityInverted ? 1f : -1f; // For position comparisons

        // Method 1: Overlap circle check (most reliable)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);

        foreach (Collider2D col in colliders)
        {
            // Skip our own collider
            if (col.gameObject == gameObject)
                continue;

            // Check if this collider is in the direction of gravity
            if (gravityInverted)
            {
                // When inverted, check if collider is above us (ceiling is new ground)
                if (col.bounds.min.y > transform.position.y)
                {
                    isGrounded = true;
                    if (showDebugInfo)
                    {
                        Debug.Log($"{gameObject.name} - GROUNDED on {col.gameObject.name} (Overlap - Inverted)");
                    }
                    break;
                }
            }
            else
            {
                // Normal: check if collider is below us
                if (col.bounds.max.y < transform.position.y)
                {
                    isGrounded = true;
                    if (showDebugInfo)
                    {
                        Debug.Log($"{gameObject.name} - GROUNDED on {col.gameObject.name} (Overlap - Normal)");
                    }
                    break;
                }
            }
        }

        // Method 2: Raycast as backup (if overlap didn't find anything)
        if (!isGrounded)
        {
            // Cast in the direction of gravity
            float rayOffset = gravityInverted ? 0.3f : -0.3f;
            Vector2 rayStart = new Vector2(transform.position.x, transform.position.y + rayOffset);
            RaycastHit2D hit = Physics2D.Raycast(rayStart, checkDirection, 0.4f, groundLayer);

            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                isGrounded = true;
                if (showDebugInfo)
                {
                    Debug.Log($"{gameObject.name} - GROUNDED on {hit.collider.gameObject.name} (Raycast - {(gravityInverted ? "Inverted" : "Normal")}) at distance {hit.distance}");
                }
            }
        }

        // Debug logging every frame if enabled
        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name} - Frame Check: Grounded={isGrounded}, Gravity={(gravityInverted ? "Inverted" : "Normal")}, Velocity.y={rb.linearVelocity.y:F2}, Position.y={transform.position.y:F2}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize ground check in editor
        if (groundCheck != null)
        {
            // Check if gravity is inverted
            bool gravityInverted = GravityManager.Instance != null && GravityManager.Instance.isGravityInverted;

            // Red when not grounded, green when grounded
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

            // Draw raycast line in the direction of gravity
            float rayOffset = gravityInverted ? 0.3f : -0.3f;
            Vector3 rayDirection = gravityInverted ? Vector3.up : Vector3.down;
            Vector3 rayStart = new Vector3(transform.position.x, transform.position.y + rayOffset, transform.position.z);

            Gizmos.color = isGrounded ? Color.green : Color.yellow;
            Gizmos.DrawLine(rayStart, rayStart + rayDirection * 0.4f);

            // Draw player reference line (bottom when normal, top when inverted)
            Gizmos.color = Color.cyan;
            float width = 0.5f;
            float referenceOffset = gravityInverted ? 0.4f : -0.4f;
            Vector3 refLeft = transform.position + Vector3.left * width + Vector3.up * referenceOffset;
            Vector3 refRight = transform.position + Vector3.right * width + Vector3.up * referenceOffset;
            Gizmos.DrawLine(refLeft, refRight);
        }
    }
}
