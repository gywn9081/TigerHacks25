using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{

    Animator animator;
    Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float speed = Mathf.Abs(rb.linearVelocity.x);
        bool isJumping = rb.linearVelocity.y > 0.1f || rb.linearVelocity.y < -0.1f;

        animator.SetFloat("Speed", speed);
        animator.SetBool("IsJumping", isJumping);
    }
}
