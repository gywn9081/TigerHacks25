# Gravity Flip System - Integration Guide

## Overview

This gravity flip system allows players to trigger buttons that invert gravity for **ALL players** in the game. The system is designed to be modular and easy to integrate with your teammate's work.

---

## System Components

### 1. **GravityManager.cs** (Singleton)
The central manager that controls gravity state for the entire game.

**Key Features:**
- Singleton pattern - only one instance exists
- Tracks global gravity state (normal or inverted)
- Automatically applies gravity to all players when flipped
- Flips player sprites upside-down when gravity inverts

**Public API:**
```csharp
// Flip gravity (toggle)
GravityManager.Instance.FlipGravity();

// Set gravity to specific state (not toggle)
GravityManager.Instance.SetGravity(bool inverted);

// Get current gravity scale value
float gravity = GravityManager.Instance.GetCurrentGravityScale();

// Check if gravity is inverted
bool isInverted = GravityManager.Instance.isGravityInverted;

// Register a new player (auto-syncs with current gravity state)
GravityManager.Instance.RegisterPlayer(PlayerController player);
```

### 2. **GravityButton.cs** (Component)
A trigger that flips gravity when a player touches it.

**Key Features:**
- Automatically detects player collision
- Visual feedback (changes color when pressed)
- Configurable: reusable or one-time use
- Cooldown system to prevent spam
- Optional audio support

**Inspector Settings:**
- `isReusable` - Can be pressed multiple times?
- `cooldownTime` - Time between presses (if reusable)
- `unpressedSprite/pressedSprite` - Visual feedback sprites
- `unpressedColor/pressedColor` - Color tinting
- `pressSound` - Audio clip to play on press

### 3. **PlayerController Updates**
Players automatically register with GravityManager on Start() and sync with current gravity state.

---

## Quick Start - Using in Your Scene

### Method 1: Automatic Setup (Easiest)
1. Run **`TigerHacks → Setup 2-Player Scene`** in Unity
2. A gravity button will be created automatically at position (0, -1, 0)
3. Press Play and run into the yellow button to test!

### Method 2: Manual Setup
1. **Create GravityManager:**
   - Create empty GameObject named "GravityManager"
   - Add `GravityManager.cs` component
   - (Or just let it auto-create itself - it's a singleton!)

2. **Create a Gravity Button:**
   - Create any GameObject (Cube, Sprite, etc.)
   - Add a 2D Collider (BoxCollider2D, CircleCollider2D, etc.)
   - **Check "Is Trigger" on the collider**
   - Add `GravityButton.cs` component
   - Configure settings in Inspector

3. **Players auto-register**, no manual setup needed!

---

## Integration with Teammate's Code

### Scenario 1: Your Teammate Has Their Own Player Scripts

**Option A: Minimal Integration** (Easiest)
Just call the GravityManager from their code:

```csharp
// In their player script, when spawning:
void Start()
{
    Rigidbody2D rb = GetComponent<Rigidbody2D>();

    // Sync with current gravity state
    if (GravityManager.Instance != null)
    {
        rb.gravityScale = GravityManager.Instance.GetCurrentGravityScale();
    }
}
```

**Option B: Event Subscription** (More robust)
Subscribe to gravity change events:

```csharp
// In their player script:
void OnEnable()
{
    if (GravityManager.Instance != null)
    {
        GravityManager.Instance.OnGravityFlip += HandleGravityFlip;
    }
}

void OnDisable()
{
    if (GravityManager.Instance != null)
    {
        GravityManager.Instance.OnGravityFlip -= HandleGravityFlip;
    }
}

void HandleGravityFlip(bool isInverted)
{
    Rigidbody2D rb = GetComponent<Rigidbody2D>();
    rb.gravityScale = isInverted ? -2.5f : 2.5f;

    // Flip sprite if needed
    if (isInverted)
    {
        transform.localScale = new Vector3(
            transform.localScale.x,
            -Mathf.Abs(transform.localScale.y),
            transform.localScale.z
        );
    }
    else
    {
        transform.localScale = new Vector3(
            transform.localScale.x,
            Mathf.Abs(transform.localScale.y),
            transform.localScale.z
        );
    }
}
```

### Scenario 2: Your Teammate Has Custom Button/Trigger Logic

They can call `GravityManager.Instance.FlipGravity()` from anywhere:

```csharp
// In their trigger script:
void OnSomeEvent()
{
    GravityManager.Instance.FlipGravity();
}
```

### Scenario 3: Different Scenes/Levels

The GravityManager persists between scenes (DontDestroyOnLoad). To reset gravity:

```csharp
// At start of new level:
void Start()
{
    GravityManager.Instance.SetGravity(false); // Reset to normal
}
```

---

## Customization Examples

### Change Gravity Values
```csharp
// In Inspector or code:
GravityManager.Instance.normalGravity = 3.0f;   // Stronger gravity
GravityManager.Instance.invertedGravity = -3.0f;
```

### Make One-Time-Use Buttons
```csharp
// In Inspector:
GravityButton.isReusable = false;
```

### Create Multiple Buttons with Different Behaviors
```csharp
// Button 1: Quick toggle
button1.isReusable = true;
button1.cooldownTime = 0.5f;

// Button 2: Slow toggle
button2.isReusable = true;
button2.cooldownTime = 3.0f;

// Button 3: One-time only
button3.isReusable = false;
```

### Trigger Gravity from Code
```csharp
// Call from anywhere:
GravityManager.Instance.FlipGravity();

// Or set to specific state:
GravityManager.Instance.SetGravity(true);  // Invert
GravityManager.Instance.SetGravity(false); // Normal
```

---

## File List for Transfer

When sharing with your teammate, they need these files:

**Core Scripts:**
1. `Assets/Scripts/GravityManager.cs` - Main singleton manager
2. `Assets/Scripts/GravityButton.cs` - Button component

**Optional (if they don't have their own):**
3. `Assets/Scripts/PlayerController.cs` - Updated with gravity registration

**Not needed:**
- `SceneSetupEditor.cs` - They can set up manually

---

## Testing Checklist

- [ ] GravityManager exists in scene (or auto-creates)
- [ ] Gravity button has collider set to "Is Trigger"
- [ ] Run into button - does gravity flip?
- [ ] Do both players flip at the same time?
- [ ] Do player sprites flip upside down?
- [ ] Can you jump on ceiling when inverted?
- [ ] Button changes color when pressed?
- [ ] Cooldown works (can't spam button)?

---

## Troubleshooting

**Q: Gravity doesn't flip when I hit the button**
- Check that button's collider has "Is Trigger" enabled
- Check Console for error messages
- Make sure player has PlayerController or Rigidbody2D

**Q: Only one player flips gravity**
- Check that both players have Rigidbody2D
- Check that both call `GravityManager.Instance.RegisterPlayer(this)` in Start()

**Q: Players don't flip upside down**
- Gravity scale is changing, but sprite flip code might be missing
- Check GravityManager.ApplyGravityToAllPlayers() is being called

**Q: Button can be spammed**
- Increase `cooldownTime` in Inspector
- Make sure `isReusable` is checked

**Q: Multiple GravityManagers exist**
- The singleton should prevent this, check for errors in Console
- Delete extra GravityManager objects manually

---

## Advanced: Custom Gravity Effects

You can subscribe to the gravity flip event for custom effects:

```csharp
void Start()
{
    GravityManager.Instance.OnGravityFlip += OnGravityChanged;
}

void OnGravityChanged(bool isInverted)
{
    if (isInverted)
    {
        // Spawn particle effect
        // Play sound
        // Shake camera
        // Change background color
    }
}
```

---

## Questions?

The system is designed to be self-contained and portable. Your teammate can:
1. Copy GravityManager.cs and GravityButton.cs
2. Call `GravityManager.Instance.FlipGravity()` from their code
3. Done!

No complex setup or dependencies required!
