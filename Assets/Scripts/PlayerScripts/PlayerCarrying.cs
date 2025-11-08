using UnityEngine;
using System.Collections.Generic;

/*

We are using this approach to carrying the movement of the bottom player in a stack upwards to allow for "carrying" but also maintaining independant movement.

*/

public class PlayerCarrierRecursive : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private Vector2 detectionBoxSize = new Vector2(0.9f, 0.1f);
    [SerializeField] private Vector2 detectionBoxOffset = new Vector2(0f, 0.5f);
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Stack Settings")]
    [SerializeField] private int maxStackDepth = 10; // Prevent infinite loops
    
    private Vector3 lastPosition;
    private bool isBeingCarried = false;
    private static HashSet<Transform> currentlyProcessing = new HashSet<Transform>();
    
    private void Start()
    {
        lastPosition = transform.position;
    }
    
    private void LateUpdate()
    {
        // Only process if we're not already being processed (prevent circular dependencies)
        if (isBeingCarried)
        {
            isBeingCarried = false;
            lastPosition = transform.position;
            return;
        }
        
        Vector3 movementDelta = transform.position - lastPosition;
        
        if (movementDelta.magnitude > 0.001f)
        {
            // Clear the processing set for this frame
            currentlyProcessing.Clear();
            currentlyProcessing.Add(transform);
            
            // Recursively move all players on top
            MovePlayersOnTopRecursive(movementDelta, 0);
        }
        
        lastPosition = transform.position;
    }
    
    private void MovePlayersOnTopRecursive(Vector3 delta, int depth)
    {
        // Safety check to prevent infinite recursion
        if (depth >= maxStackDepth)
        {
            Debug.LogWarning($"Max stack depth reached at {gameObject.name}");
            return;
        }
        
        // Detect players directly on top of this player
        Vector2 boxCenter = (Vector2)transform.position + detectionBoxOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, detectionBoxSize, 0f, playerLayer);
        
        foreach (Collider2D hit in hits)
        {
            Transform player = hit.transform;
            
            // Skip self and already processed players
            if (player == transform || currentlyProcessing.Contains(player))
                continue;
            
            // Check if player is actually above us
            if (player.position.y > transform.position.y)
            {
                // Mark as being carried and processed
                PlayerCarrierRecursive carrierScript = player.GetComponent<PlayerCarrierRecursive>();
                if (carrierScript != null)
                {
                    carrierScript.isBeingCarried = true;
                    currentlyProcessing.Add(player);
                }
                
                // Move this player
                player.position += delta;
                
                // Sync Rigidbody2D if present
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.position += (Vector2)delta;
                }
                
                // RECURSIVELY move anyone on top of this player
                if (carrierScript != null)
                {
                    carrierScript.MovePlayersOnTopRecursive(delta, depth + 1);
                }
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 boxCenter = (Vector2)transform.position + detectionBoxOffset;
        Gizmos.DrawWireCube(boxCenter, detectionBoxSize);
    }
}