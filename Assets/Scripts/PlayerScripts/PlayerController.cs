using UnityEngine;
using UnityEngine.InputSystem;
using System;
public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private int playerNumber = 1;
    [SerializeField] private Color playerColor = Color.white;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] public float acceleration = 40f;

    [SerializeField] public float normalAcceleration = 50f;
    
    [Header("Jump Check")]
    [SerializeField] private Transform jumpCheck;
    [SerializeField] private float jumpCheckRadius = 0.2f;
    [SerializeField] private LayerMask jumpableLayers;
    [SerializeField] private bool isJumpable;

    [Header("Material Conditions")]
    [SerializeField] private Transform iceCheck;
    [SerializeField] private float iceCheckRadius = 0.2f;
    [SerializeField] private float iceScaling = 0.1f;
    [SerializeField] private LayerMask iceLayers;
    [SerializeField] private bool isIcy;

    [Header("Default Material")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private bool isGrounded;
    private Collider2D playerCollider;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private Vector2 moveInput;
    private PlayerInput playerInput;

    [Header("Items")]
    [SerializeField] private bool jumpBoots;
    private bool jumpBootsCooldown;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        // Wire up the events manually if needed
        if (playerInput != null)
        {
            // The events will be connected through the Inspector when using "Invoke Unity Events"
            Debug.Log($"PlayerInput initialized for {gameObject.name}");
        }

        // Set acceleration
        acceleration = normalAcceleration;
    }
    
    void Start()
    {
        // Set player color
        spriteRenderer.color = playerColor;
        // Set ref to child collider
        playerCollider = GetComponent<Collider2D>();
    }
    
    void Update()
    {
        // Flip sprite based on movement direction
        if (moveInput.x < 0)
            spriteRenderer.flipX = true;
        else if (moveInput.x > 0)
            spriteRenderer.flipX = false;
    }

    void FixedUpdate()
    {
        // Check if jumpable
        CheckJumpable();
        // Check if grounded
        CheckGrounded();
        // Check if icy
        CheckIcy();
        // Make sure material changes are smooth
        SpeedSmoothing();

        // Calculate target velocity
        float targetVelocityX = moveInput.x * moveSpeed;
        
        // Smoothly move towards target velocity instead of setting it directly
        float newVelocityX = Mathf.MoveTowards(
            rb.linearVelocity.x, 
            targetVelocityX, 
            acceleration * Time.fixedDeltaTime
        );
        
        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
    }

    // Jumpable check
    void CheckJumpable()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(jumpCheck.position, jumpCheckRadius, jumpableLayers);

        isJumpable = false;

        foreach (Collider2D collider in hitColliders)
        {
            // If we find any collider that isn't our own, we're on a jumpable surface
            if (collider != playerCollider)
            {
                isJumpable = true;
                // We set this to false just to indicate we have stepped on a jumpable surface
                jumpBootsCooldown = false;
                break; // Found valid surface, no need to keep checking
            }
        }
    }
    // Icy check
    void CheckIcy()
    {
        isIcy = Physics2D.OverlapCircle(iceCheck.position, iceCheckRadius, iceLayers);
    }

    // Check for materials that don't have special properties
    void CheckGrounded()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayers);

        isGrounded = false;

        foreach (Collider2D collider in hitColliders)
        {
            // If we find any collider that isn't our own, we're on a jumpable surface
            if (collider != playerCollider)
            {
                isGrounded = true;
                break; // Found valid surface, no need to keep checking
            }
        }
    }

    // Smooth acceleration changing so when jumping don't accelerate fast
    void SpeedSmoothing()
    {
        if (isIcy)
        {
            acceleration = normalAcceleration * iceScaling;
        }
        else if (isJumpable)
        {
            acceleration = normalAcceleration;
        }
    }

    // Input System callback for movement (called by Send Messages)
    // Note: Method name must match the action name in InputActions (case-sensitive!)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    
    // Input System callback for jump (called by Send Messages)
    public void OnJump(InputValue value)
    {
        if (!value.isPressed) return;
        if (isJumpable)
        {
            Jump();
        }
        // Allow for double jump boots to be used
        else if (jumpBoots && !jumpBootsCooldown)
        {
            Jump();
            jumpBootsCooldown = true;
        }
    }
    
    // Input System callback for action button (called by Send Messages)
    public void OnAction(InputValue value)
    {
        if (value.isPressed)
        {
            // Add your action logic here
            Debug.Log($"Player {playerNumber} performed action!");
        }
    }
    
    void Jump()
    {           // Check if gravity is inverted and adjust jump direction
        float jumpDirection = 1f;
        if (GravityManager.Instance != null && GravityManager.Instance.isGravityInverted)
        {
            jumpDirection = -1f; // Jump downward when gravity is inverted
        }
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * jumpDirection);
    }
    
    // Visualize ground check in editor
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
    
    public void SetPlayerNumber(int number)
    {
        playerNumber = number;
    }
    
    public void SetPlayerColor(Color color)
    {
        playerColor = color;
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }
    
    public int GetPlayerNumber()
    {
        return playerNumber;
    }

    public PlayerInput GetPlayerInput()
    {
        return playerInput;
    }
    
    /*
        Trigger death stuff
    */
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object is on the Deadly layer
        if (other.gameObject.layer == LayerMask.NameToLayer("Deadly"))
        {
            OnDeath();
        }
    }
    public void OnDeath()
    {
        DeathSystem.TriggerPlayerDeath();
    }

}