using UnityEngine;
using UnityEditor;

public class SceneSetupEditor : MonoBehaviour
{
    [MenuItem("TigerHacks/Setup 2-Player Scene")]
    public static void SetupScene()
    {
        Debug.Log("=== Starting Scene Setup ===");

        // Clear existing objects (except camera and lights)
        ClearScene();

        // Create Player 1
        Debug.Log("Creating Player 1...");
        GameObject player1 = CreatePlayer("Player1", new Vector3(-2, 2, 0), KeyCode.A, KeyCode.D, KeyCode.W, Color.blue);

        // Create Player 2
        Debug.Log("Creating Player 2...");
        GameObject player2 = CreatePlayer("Player2", new Vector3(2, 2, 0), KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, Color.red);

        // Create physics material for players (no friction)
        PhysicsMaterial2D playerMaterial = CreatePlayerPhysicsMaterial();

        // Apply physics material to players
        player1.GetComponent<BoxCollider2D>().sharedMaterial = playerMaterial;
        player2.GetComponent<BoxCollider2D>().sharedMaterial = playerMaterial;

        // Create Ground
        Debug.Log("Creating Ground...");
        CreateGround();

        // Create Roof
        Debug.Log("Creating Roof...");
        CreateRoof();

        // Create Platforms
        Debug.Log("Creating Platforms...");
        CreatePlatform(new Vector3(-4, -1, 0), new Vector2(3, 0.5f));
        CreatePlatform(new Vector3(4, 0, 0), new Vector2(3, 0.5f));
        CreatePlatform(new Vector3(0, 1.5f, 0), new Vector2(2, 0.5f));

        // Create Gravity Button
        Debug.Log("Creating Gravity Button...");
        CreateGravityButton(new Vector3(0, -1, 0));

        // Create GravityManager
        Debug.Log("Creating GravityManager...");
        SetupGravityManager();

        // Setup Camera
        Debug.Log("Setting up Camera...");
        SetupCamera(player1, player2);

        Debug.Log("=== Scene Setup Complete! ===");

        // Verify required layers exist
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        int playerLayerIndex = LayerMask.NameToLayer("Player");

        string missingLayers = "";
        if (groundLayerIndex == -1) missingLayers += "- Ground\n";
        if (playerLayerIndex == -1) missingLayers += "- Player\n";

        if (missingLayers.Length > 0)
        {
            EditorUtility.DisplayDialog("Warning!",
                "The following layers do NOT exist:\n\n" +
                missingLayers + "\n" +
                "Please:\n" +
                "1. Go to Edit → Project Settings → Tags and Layers\n" +
                "2. Add 'Ground' to Layer 6\n" +
                "3. Add 'Player' to Layer 7\n" +
                "4. Run this setup again\n\n" +
                "Without these layers, jumping will not work!",
                "OK");
            return;
        }

        EditorUtility.DisplayDialog("Success",
            "2-Player scene setup complete!\n\n" +
            "Controls:\n" +
            "Player 1 (Blue): WASD\n" +
            "Player 2 (Red): Arrow Keys\n\n" +
            $"Ground layer: Layer {groundLayerIndex}\n" +
            $"Player layer: Layer {playerLayerIndex}\n\n" +
            "Players can jump on each other!",
            "OK");
    }

    private static void ClearScene()
    {
        // Find and destroy old player and ground objects
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Player") ||
                obj.name.Contains("Ground") ||
                obj.name.Contains("Platform") ||
                obj.name.Contains("Roof") ||
                obj.name.Contains("GravityButton") ||
                obj.name.Contains("GravityManager"))
            {
                DestroyImmediate(obj);
            }
        }
    }

    private static PhysicsMaterial2D CreatePlayerPhysicsMaterial()
    {
        // Create a physics material with zero friction so players slide off walls
        PhysicsMaterial2D material = new PhysicsMaterial2D("PlayerMaterial");
        material.friction = 0f; // No friction
        material.bounciness = 0f; // No bounce

        // Save it as an asset
        if (!AssetDatabase.IsValidFolder("Assets/Physics"))
        {
            AssetDatabase.CreateFolder("Assets", "Physics");
        }

        string path = "Assets/Physics/PlayerMaterial.physicsMaterial2D";

        // Delete old one if exists
        if (AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        AssetDatabase.CreateAsset(material, path);
        AssetDatabase.SaveAssets();

        Debug.Log("Created Player Physics Material with zero friction");
        return material;
    }

    private static GameObject CreatePlayer(string name, Vector3 position, KeyCode left, KeyCode right, KeyCode jump, Color color)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = name;
        player.transform.position = position;
        player.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        // Set player to Player layer
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1)
        {
            player.layer = playerLayer;
            Debug.Log($"{name} set to Player layer ({playerLayer})");
        }

        // Set color
        Renderer renderer = player.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        renderer.material = mat;

        // Remove 3D collider
        DestroyImmediate(player.GetComponent<BoxCollider>());

        // Add 2D components
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.gravityScale = 2.5f; // Slightly increased gravity for better feel
        rb.linearDamping = 0f; // No linear drag

        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();

        // Add PlayerController
        PlayerController controller = player.AddComponent<PlayerController>();
        controller.leftKey = left;
        controller.rightKey = right;
        controller.jumpKey = jump;
        controller.moveSpeed = 5f;
        controller.jumpForce = 10f;

        // Create ground check - positioned lower and larger radius to account for floating
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.parent = player.transform;
        groundCheck.transform.localPosition = new Vector3(0, -0.45f, 0); // Slightly higher to catch the gap

        controller.groundCheck = groundCheck.transform;
        controller.groundCheckRadius = 0.35f; // Larger radius to detect through the gap
        controller.groundLayer = LayerMask.GetMask("Ground", "Player"); // Check both Ground and Player layers

        Debug.Log($"Created {name} at {position} with ground layer mask: {controller.groundLayer.value}");
        return player;
    }

    private static void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0, -3, 0);
        ground.transform.localScale = new Vector3(20, 1, 1);

        // Try to set layer to Ground if it exists
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1)
        {
            ground.layer = groundLayer;
            Debug.Log($"Ground object set to layer: {groundLayer} ({LayerMask.LayerToName(groundLayer)})");
        }
        else
        {
            Debug.LogError("Ground layer does not exist! Please add it in Tags and Layers.");
        }

        // Set color
        Renderer renderer = ground.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.3f, 0.5f, 0.3f);
        renderer.material = mat;

        // Remove 3D collider and add 2D
        DestroyImmediate(ground.GetComponent<BoxCollider>());
        ground.AddComponent<BoxCollider2D>();

        Debug.Log($"Created Ground on layer {ground.layer}");
    }

    private static void CreateRoof()
    {
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.position = new Vector3(0, 8, 0);
        roof.transform.localScale = new Vector3(20, 1, 1);

        // Try to set layer to Ground if it exists
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1)
        {
            roof.layer = groundLayer;
        }

        // Set color (darker gray for roof)
        Renderer renderer = roof.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.2f, 0.2f, 0.2f);
        renderer.material = mat;

        // Remove 3D collider and add 2D
        DestroyImmediate(roof.GetComponent<BoxCollider>());
        roof.AddComponent<BoxCollider2D>();

        Debug.Log("Created Roof");
    }

    private static void CreatePlatform(Vector3 position, Vector2 size)
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "Platform";
        platform.transform.position = position;
        platform.transform.localScale = new Vector3(size.x, size.y, 1);

        // Try to set layer to Ground if it exists
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1)
        {
            platform.layer = groundLayer;
        }

        // Set color
        Renderer renderer = platform.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.4f, 0.3f, 0.2f);
        renderer.material = mat;

        // Remove 3D collider and add 2D
        DestroyImmediate(platform.GetComponent<BoxCollider>());
        platform.AddComponent<BoxCollider2D>();

        Debug.Log($"Created Platform at {position}");
    }

    private static void CreateGravityButton(Vector3 position)
    {
        GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
        button.name = "GravityButton";
        button.transform.position = position;
        button.transform.localScale = new Vector3(1f, 1f, 1f);

        // Set color (yellow/orange for visibility)
        Renderer renderer = button.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 0.8f, 0.2f); // Yellow/orange
        renderer.material = mat;

        // Remove 3D collider and add 2D trigger
        DestroyImmediate(button.GetComponent<BoxCollider>());
        BoxCollider2D collider = button.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        // Add GravityButton script
        GravityButton buttonScript = button.AddComponent<GravityButton>();
        buttonScript.isReusable = true;
        buttonScript.cooldownTime = 1f;
        buttonScript.unpressedColor = new Color(1f, 0.8f, 0.2f);
        buttonScript.pressedColor = new Color(0.2f, 1f, 0.2f);

        Debug.Log($"Created Gravity Button at {position}");
    }

    private static void SetupGravityManager()
    {
        // Check if GravityManager already exists
        GravityManager existing = GameObject.FindObjectOfType<GravityManager>();
        if (existing != null)
        {
            Debug.Log("GravityManager already exists");
            return;
        }

        // Create GravityManager GameObject
        GameObject managerGO = new GameObject("GravityManager");
        GravityManager manager = managerGO.AddComponent<GravityManager>();
        manager.normalGravity = 2.5f;
        manager.invertedGravity = -2.5f;

        Debug.Log("Created GravityManager");
    }

    private static void SetupCamera(GameObject player1, GameObject player2)
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Remove existing camera script if any
            MultiplayerCamera existing = mainCam.GetComponent<MultiplayerCamera>();
            if (existing != null)
            {
                DestroyImmediate(existing);
            }

            // Add new one
            MultiplayerCamera mpCam = mainCam.gameObject.AddComponent<MultiplayerCamera>();
            mpCam.player1 = player1.transform;
            mpCam.player2 = player2.transform;
            mpCam.smoothSpeed = 0.125f;
            mpCam.minZoom = 5f;
            mpCam.maxZoom = 15f;
            mpCam.zoomLimiter = 50f;

            Debug.Log("Camera setup complete");
        }
        else
        {
            Debug.LogError("No Main Camera found in scene!");
        }
    }

    [MenuItem("TigerHacks/Add Required Layers")]
    public static void AddRequiredLayers()
    {
        EditorUtility.DisplayDialog("Add Required Layers",
            "Please add layers manually:\n\n" +
            "1. Go to Edit → Project Settings → Tags and Layers\n" +
            "2. Add 'Ground' to Layer 6\n" +
            "3. Add 'Player' to Layer 7\n" +
            "4. Close Project Settings\n" +
            "5. Then run 'TigerHacks → Setup 2-Player Scene'\n\n" +
            "The Player layer allows players to jump on each other!",
            "OK");
    }

    [MenuItem("TigerHacks/Fix Physics Settings")]
    public static void FixPhysicsSettings()
    {
        // Adjust Physics2D settings to reduce floating
        Physics2D.defaultContactOffset = 0.001f; // Reduce from default 0.01

        Debug.Log("Physics2D settings adjusted:");
        Debug.Log($"  Default Contact Offset: {Physics2D.defaultContactOffset}");

        EditorUtility.DisplayDialog("Physics Settings Updated",
            "Physics2D settings have been adjusted to reduce floating.\n\n" +
            "Changes:\n" +
            "- Default Contact Offset: 0.001 (was 0.01)\n\n" +
            "This should make players sit flush on surfaces.",
            "OK");
    }

    [MenuItem("TigerHacks/Verify Scene Setup")]
    public static void VerifySceneSetup()
    {
        string report = "=== Scene Verification Report ===\n\n";

        // Check if Ground layer exists
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        if (groundLayerIndex == -1)
        {
            report += "❌ PROBLEM: 'Ground' layer does NOT exist!\n";
            report += "   Fix: Edit → Project Settings → Tags and Layers → Add 'Ground' to Layer 6\n\n";
        }
        else
        {
            report += $"✓ Ground layer exists on Layer {groundLayerIndex}\n";
        }

        // Check if Player layer exists
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        if (playerLayerIndex == -1)
        {
            report += "❌ PROBLEM: 'Player' layer does NOT exist!\n";
            report += "   Fix: Edit → Project Settings → Tags and Layers → Add 'Player' to Layer 7\n\n";
        }
        else
        {
            report += $"✓ Player layer exists on Layer {playerLayerIndex}\n\n";
        }

        // Check for ground objects in scene
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int groundObjectsFound = 0;
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Ground") || obj.name.Contains("Platform") || obj.name.Contains("Roof"))
            {
                groundObjectsFound++;
                string layerName = LayerMask.LayerToName(obj.layer);
                report += $"  {obj.name}: Layer {obj.layer} ({layerName})\n";

                if (obj.layer != groundLayerIndex && groundLayerIndex != -1)
                {
                    report += $"    ⚠️ WARNING: Should be on Ground layer ({groundLayerIndex})\n";
                }
            }
        }

        if (groundObjectsFound == 0)
        {
            report += "❌ No ground objects found in scene!\n";
        }
        else
        {
            report += $"\n✓ Found {groundObjectsFound} ground-related objects\n\n";
        }

        // Check for players
        PlayerController[] players = GameObject.FindObjectsOfType<PlayerController>();
        report += $"\nPlayers found: {players.Length}\n";
        foreach (PlayerController player in players)
        {
            string playerLayerName = LayerMask.LayerToName(player.gameObject.layer);
            report += $"  {player.gameObject.name}:\n";
            report += $"    Object Layer: {player.gameObject.layer} ({playerLayerName})\n";
            report += $"    Ground Layer Mask: {player.groundLayer.value}\n";
            report += $"    Ground Check: {(player.groundCheck != null ? "Set" : "MISSING!")}\n";

            // Check if player is on correct layer (use already declared playerLayerIndex)
            if (playerLayerIndex != -1 && player.gameObject.layer != playerLayerIndex)
            {
                report += $"    ⚠️ WARNING: Should be on Player layer ({playerLayerIndex})\n";
            }

            // Check if ground layer mask includes both Ground and Player
            int expectedMask = LayerMask.GetMask("Ground", "Player");
            if (player.groundLayer.value != expectedMask && groundLayerIndex != -1 && playerLayerIndex != -1)
            {
                report += $"    ⚠️ WARNING: Ground mask should be {expectedMask} (Ground + Player)\n";
            }
        }

        Debug.Log(report);
        EditorUtility.DisplayDialog("Scene Verification", report, "OK");
    }
}
