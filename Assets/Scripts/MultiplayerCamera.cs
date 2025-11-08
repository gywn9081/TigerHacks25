using UnityEngine;

public class MultiplayerCamera : MonoBehaviour
{
    [Header("Target Players")]
    public Transform player1;
    public Transform player2;

    [Header("Camera Settings")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float minZoom = 5f;
    public float maxZoom = 15f;
    public float zoomLimiter = 50f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (player1 == null || player2 == null)
            return;

        // Find the center point between both players
        Vector3 centerPoint = GetCenterPoint();

        // Move camera to center point
        Vector3 newPosition = centerPoint + offset;
        transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed);

        // Adjust zoom based on distance between players
        AdjustZoom();
    }

    private Vector3 GetCenterPoint()
    {
        Vector3 center = (player1.position + player2.position) / 2f;
        return center;
    }

    private void AdjustZoom()
    {
        float distance = Vector3.Distance(player1.position, player2.position);
        float newZoom = Mathf.Lerp(minZoom, maxZoom, distance / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, smoothSpeed);
    }
}
