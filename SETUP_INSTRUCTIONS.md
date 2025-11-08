 istn# 2-Player Platformer Setup Instructions

## Quick Setup (Automated)

1. **Add Ground Layer:**
   - Go to `Edit -> Project Settings -> Tags and Layers`
   - Add "Ground" to Layer 6

2. **Open Unity and your scene** (Assets/Scenes/SampleScene)

3. **Create an empty GameObject:**
   - Right-click in Hierarchy -> Create Empty
   - Name it "GameSetup"
   - Add the `GameSetup` script component to it
   - Right-click on the GameSetup component
   - Select "Setup Game Scene" from the context menu

4. **Done!** Your scene now has:
   - Two players (blue and red cubes)
   - Ground and platforms
   - Camera that follows both players

## Controls

- **Player 1 (Blue):**
  - Move: A (left) / D (right)
  - Jump: W

- **Player 2 (Red):**
  - Move: Left Arrow / Right Arrow
  - Jump: Up Arrow

## Manual Setup (Alternative)

If the automatic setup doesn't work, you can set up manually:

### 1. Create Players

**Player 1:**
- Create a Cube (GameObject -> 3D Object -> Cube)
- Name it "Player1"
- Scale: (0.8, 0.8, 1)
- Position: (-2, 2, 0)
- Remove Box Collider (3D)
- Add Rigidbody2D (freeze rotation Z)
- Add Box Collider 2D
- Add `PlayerController` script
  - Left Key: A
  - Right Key: D
  - Jump Key: W
- Create child Empty GameObject named "GroundCheck"
  - Position: (0, -0.5, 0)
- In PlayerController, drag GroundCheck to the Ground Check field
- Set Ground Check Radius to 0.2
- Set Ground Layer to "Ground" and "Default"

**Player 2:**
- Same as Player 1, but:
  - Name: "Player2"
  - Position: (2, 2, 0)
  - Color: Red (change material)
  - Left Key: LeftArrow
  - Right Key: RightArrow
  - Jump Key: UpArrow

### 2. Create Ground

- Create a Cube
- Name: "Ground"
- Position: (0, -3, 0)
- Scale: (20, 1, 1)
- Layer: Ground
- Remove Box Collider (3D)
- Add Box Collider 2D

### 3. Create Platforms

Create several cubes for platforms:
- Remove Box Collider (3D)
- Add Box Collider 2D
- Set Layer to "Ground"
- Position them at different heights

### 4. Setup Camera

- Select Main Camera
- Add `MultiplayerCamera` script
- Drag Player1 and Player2 to the player slots

## Testing

Press Play! Both players should be able to move and jump independently. The camera should follow both players and zoom out when they move apart.

## Tips for Hackathon Development

1. **Add sprites:** Replace the cubes with actual sprites by using SpriteRenderer instead of MeshRenderer
2. **Add collectibles:** Create coin/pickup objects with OnTriggerEnter2D
3. **Add enemies:** Create simple enemy AI with patrol behavior
4. **Add win condition:** Create a goal/flag that both players must reach
5. **Add sound:** Import audio clips and use AudioSource.PlayOneShot()
6. **Polish:** Add particle effects, animations, and UI

## Troubleshooting

- **Players fall through ground:** Make sure ground has Layer "Ground" and players have it in their Ground Layer mask
- **Players can't jump:** Check that GroundCheck is positioned correctly (below the player)
- **Camera doesn't follow:** Make sure both player references are set in MultiplayerCamera component
- **Controls don't work:** Verify the key codes are set correctly in each PlayerController
