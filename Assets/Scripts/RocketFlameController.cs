using UnityEngine;

public class RocketFlameController : MonoBehaviour
{
    [SerializeField] private Sprite[] startupFrames; // Frames 1-12
    [SerializeField] private Sprite[] loopFrames;    // Frames 9-12
    [SerializeField] private float frameRate = 12f;  // Frames per second

    private SpriteRenderer spriteRenderer;
    private int currentFrame = 0;
    private float frameTimer = 0f;
    private bool isInLoop = false;
    private bool isPlaying = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("RocketFlameController requires a SpriteRenderer component!");
            enabled = false;
            return;
        }

        if (startupFrames.Length == 0 || loopFrames.Length == 0)
        {
            Debug.LogError("Please assign both startup frames and loop frames in the Inspector!");
            enabled = false;
            return;
        }

        // Start hidden until activated
        spriteRenderer.enabled = false;
        isPlaying = false;
    }

    void Update()
    {
        if (!isPlaying)
            return;

        frameTimer += Time.deltaTime;

        if (frameTimer >= 1f / frameRate)
        {
            frameTimer = 0f;
            AdvanceFrame();
        }
    }

    void AdvanceFrame()
    {
        if (!isInLoop)
        {
            // Playing startup animation (frames 1-12)
            currentFrame++;

            if (currentFrame >= startupFrames.Length)
            {
                // Startup complete, switch to loop
                isInLoop = true;
                currentFrame = 0;
                spriteRenderer.sprite = loopFrames[0];
            }
            else
            {
                spriteRenderer.sprite = startupFrames[currentFrame];
            }
        }
        else
        {
            // Playing loop animation (frames 9-12)
            currentFrame++;

            if (currentFrame >= loopFrames.Length)
            {
                currentFrame = 0;
            }

            spriteRenderer.sprite = loopFrames[currentFrame];
        }
    }

    // Call this method to restart the animation from the beginning
    public void RestartAnimation()
    {
        isInLoop = false;
        currentFrame = 0;
        frameTimer = 0f;
        isPlaying = true;
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = startupFrames[0];
    }

    // Call this method to stop and hide the flame
    public void StopAnimation()
    {
        isPlaying = false;
        spriteRenderer.enabled = false;
    }
}
