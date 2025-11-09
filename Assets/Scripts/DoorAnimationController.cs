using UnityEngine;

public class DoorAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private string closeTriggerName = "Close";

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip doorCloseSound;

    private AudioSource audioSource;
    private bool isOpen = false;

    void Awake()
    {
        // Get animator if not assigned
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();

        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // Subscribe to player death events to close door
        DeathSystem.OnAnyPlayerDied += OnPlayerDeath;
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        DeathSystem.OnAnyPlayerDied -= OnPlayerDeath;
    }

    // Call this method to open the door
    public void OpenDoor()
    {
        Debug.Log($"[DoorAnimation] OpenDoor called! isOpen={isOpen}, doorAnimator={doorAnimator != null}");

        if (isOpen)
        {
            Debug.LogWarning("[DoorAnimation] Door is already open, ignoring");
            return;
        }

        if (doorAnimator == null)
        {
            Debug.LogError("[DoorAnimation] doorAnimator is NULL! Cannot open door.");
            return;
        }

        Debug.Log($"[DoorAnimation] Setting trigger '{openTriggerName}' on animator");

        isOpen = true;
        doorAnimator.SetTrigger(openTriggerName);

        Debug.Log("[DoorAnimation] Trigger set successfully");

        // Play sound effect
        if (audioSource != null && doorOpenSound != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }
    }

    // Call this method to close the door
    public void CloseDoor()
    {
        if (!isOpen || doorAnimator == null)
            return;

        Debug.Log("[DoorAnimation] Closing door");

        isOpen = false;

        // Only trigger close animation if the parameter exists
        if (HasParameter(closeTriggerName))
        {
            doorAnimator.SetTrigger(closeTriggerName);
        }
        else
        {
            // If no close animation, just reset to default state
            doorAnimator.Play("Closed", 0, 0f);
        }

        // Play sound effect
        if (audioSource != null && doorCloseSound != null)
        {
            audioSource.PlayOneShot(doorCloseSound);
        }
    }

    // Reset door on player death
    void OnPlayerDeath()
    {
        Debug.Log("[DoorAnimation] Player died - closing door");
        CloseDoor();
    }

    // Check if animator has a specific parameter
    private bool HasParameter(string paramName)
    {
        if (doorAnimator == null)
            return false;

        foreach (AnimatorControllerParameter param in doorAnimator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    // Public method to check door state
    public bool IsOpen()
    {
        return isOpen;
    }
}
