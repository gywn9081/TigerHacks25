using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameSetup : MonoBehaviour
{
    [Header("Setup Settings")]
    [SerializeField] private bool autoSetupOnStart = false;

#if UNITY_EDITOR
    [ContextMenu("Setup Game Scene")]
    public void SetupScene()
    {
        Debug.Log("Setting up game scene...");

        // Create ground layer if it doesn't exist
        SetupLayers();

        // Create Player 1
        GameObject player1 = CreatePlayer("Player1", new Vector3(-2, 2, 0), KeyCode.A, KeyCode.D, KeyCode.W, Color.blue);

        // Create Player 2
        GameObject player2 = CreatePlayer("Player2", new Vector3(2, 2, 0), KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, Color.red);

        // Create Ground
        CreateGround();

        // Create Platforms
        CreatePlatform(new Vector3(-4, -1, 0), new Vector2(3, 0.5f));
        CreatePlatform(new Vector3(4, 0, 0), new Vector2(3, 0.5f));
        CreatePlatform(new Vector3(0, 1.5f, 0), new Vector2(2, 0.5f));

        // Setup Camera
        SetupCamera(player1, player2);

        Debug.Log("Scene setup complete!");
    }

    private GameObject CreatePlayer(string name, Vector3 position, KeyCode left, KeyCode right, KeyCode jump, Color color)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = name;
        player.transform.position = position;
        player.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        player.layer = LayerMask.NameToLayer("Default");

        // Set color
        Renderer renderer = player.GetComponent<Renderer>();
        renderer.material.color = color;

        // Add Rigidbody2D
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Add BoxCollider2D (replace the 3D one)
        DestroyImmediate(player.GetComponent<BoxCollider>());
        player.AddComponent<BoxCollider2D>();

        // Add PlayerController
        PlayerController controller = player.AddComponent<PlayerController>();
        controller.leftKey = left;
        controller.rightKey = right;
        controller.jumpKey = jump;

        // Create ground check
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.parent = player.transform;
        groundCheck.transform.localPosition = new Vector3(0, -0.5f, 0);

        controller.groundCheck = groundCheck.transform;
        controller.groundCheckRadius = 0.2f;
        controller.groundLayer = LayerMask.GetMask("Ground", "Default");

        return player;
    }

    private void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0, -3, 0);
        ground.transform.localScale = new Vector3(20, 1, 1);
        ground.layer = LayerMask.NameToLayer("Ground");

        // Set color
        Renderer renderer = ground.GetComponent<Renderer>();
        renderer.material.color = new Color(0.3f, 0.5f, 0.3f);

        // Add BoxCollider2D
        DestroyImmediate(ground.GetComponent<BoxCollider>());
        ground.AddComponent<BoxCollider2D>();
    }

    private void CreatePlatform(Vector3 position, Vector2 size)
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "Platform";
        platform.transform.position = position;
        platform.transform.localScale = new Vector3(size.x, size.y, 1);
        platform.layer = LayerMask.NameToLayer("Ground");

        // Set color
        Renderer renderer = platform.GetComponent<Renderer>();
        renderer.material.color = new Color(0.4f, 0.3f, 0.2f);

        // Add BoxCollider2D
        DestroyImmediate(platform.GetComponent<BoxCollider>());
        platform.AddComponent<BoxCollider2D>();
    }

    private void SetupCamera(GameObject player1, GameObject player2)
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            MultiplayerCamera mpCam = mainCam.gameObject.GetComponent<MultiplayerCamera>();
            if (mpCam == null)
            {
                mpCam = mainCam.gameObject.AddComponent<MultiplayerCamera>();
            }

            mpCam.player1 = player1.transform;
            mpCam.player2 = player2.transform;
        }
    }

    private void SetupLayers()
    {
        // Note: Adding layers programmatically requires TagManager editing
        // For hackathon, you can manually add a "Ground" layer in Unity:
        // Edit -> Project Settings -> Tags and Layers -> Add "Ground" to layer 6
        Debug.Log("Make sure to add a 'Ground' layer in Edit -> Project Settings -> Tags and Layers");
    }
#endif

    private void Start()
    {
        if (autoSetupOnStart)
        {
#if UNITY_EDITOR
            SetupScene();
#endif
        }
    }
}
