using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private int playerNumber = 1;
    [SerializeField] private Color playerColor = Color.white;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isGrounded;
    private Vector2 moveInput;
    private PlayerInput playerInput;
    
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
    }
    
    void Start()
    {
        // Set player color
        spriteRenderer.color = playerColor;
    }
    
    void Update()
    {
        // Flip sprite based on movement direction
        if (moveInput.x < 0)
            spriteRenderer.flipX = true;
        else if (moveInput.x > 0)
            spriteRenderer.flipX = false;
        
        // Update animator parameters if animator exists
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
            animator.SetBool("IsGrounded", isGrounded);
        }
    }
    
    void FixedUpdate()
    {
        // Check if grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Apply horizontal movement
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
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
        if (value.isPressed && isGrounded)
        {
            Jump();
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
}