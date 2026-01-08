# Unity Editor Setup Guide

## Important: Manual Unity Setup Required

Since the Unity project was created programmatically, you need to complete the following steps in Unity Editor to make the game fully functional.

## Step 1: Open Project in Unity

1. Open Unity Hub
2. Click "Add" → Navigate to `MobaGame/UnityClient`
3. Open with Unity 2022.3 LTS
4. Wait for initial import (this may take a few minutes)

## Step 2: Create Scenes

### Option A: Use the Menu Helper (Recommended)

1. In Unity menu: **MOBA → Setup → Create All Scenes**
2. Then: **MOBA → Setup → Add Scenes to Build Settings**

### Option B: Manual Creation

Create the following scenes in `Assets/Scenes/`:

#### 1. LoginScene
- Add UI Canvas (GameObject → UI → Canvas)
- Add TMP InputField for username
- Add Button for login
- Add LoginScreen.cs to Canvas
- Wire up references in Inspector

#### 2. LobbyScene
- Add UI Canvas
- Add welcome text (TextMeshPro)
- Add "Quick Match" button
- Add "Exit" button
- Add LobbyScreen.cs to Canvas
- Wire up references

#### 3. HeroSelectionScene
- Add UI Canvas
- Add hero selection UI elements
- Add HeroSelectionScreen.cs to Canvas
- Create HeroData assets (see below)
- Assign hero data array

#### 4. GameScene (Most Complex)
- Create the game map:
  - Add Plane for ground (scale: 20, 1, 20)
  - Create towers (Cylinders) with Tower.cs script
  - Create crystals (Cubes) with Crystal.cs script
  - Create minion spawners with MinionSpawner.cs
  
- Add Player:
  - Create Capsule for player hero
  - Add Hero.cs script
  - Add PlayerController.cs script
  - Add CharacterController component
  
- Add Camera:
  - Position: (0, 30, -20)
  - Rotation: (60, 0, 0)
  - Follow player (optional camera script)

- Add UI:
  - Canvas with GameHUD.cs
  - Health/Mana bars (UI Sliders)
  - KDA text
  - Skill buttons
  - Virtual joystick (see below)
  - Shop panel
  - Chat panel

- Add GameManager (Empty GameObject):
  - Add GameManager.cs script

#### 5. ResultScene
- Add UI Canvas
- Add result title text
- Add stats text
- Add "Return to Lobby" button
- Add ResultScreen.cs to Canvas
- Wire up references

## Step 3: Create Hero Data Assets

In Project window: **Right-click → Create → MOBA → Hero Data**

Create 5 heroes with these configurations:

### Tank Hero
- Name: "Tank Warrior"
- Class: Tank
- Attack Type: Melee
- Max Health: 1500
- Max Mana: 400
- Attack Damage: 40
- Move Speed: 4.5
- Attack Range: 2
- Armor: 50
- Magic Resist: 35

### Fighter Hero
- Name: "Battle Fighter"
- Class: Fighter
- Attack Type: Melee
- Max Health: 1200
- Max Mana: 450
- Attack Damage: 60
- Move Speed: 5.0
- Attack Range: 2
- Armor: 40
- Magic Resist: 30

### Assassin Hero
- Name: "Shadow Assassin"
- Class: Assassin
- Attack Type: Melee
- Max Health: 900
- Max Mana: 500
- Attack Damage: 80
- Move Speed: 6.0
- Attack Range: 2
- Armor: 25
- Magic Resist: 25

### Mage Hero
- Name: "Arcane Mage"
- Class: Mage
- Attack Type: Ranged
- Max Health: 800
- Max Mana: 700
- Attack Damage: 30
- Magic Power: 100
- Move Speed: 4.5
- Attack Range: 8
- Armor: 20
- Magic Resist: 40

### Marksman Hero
- Name: "Frost Archer"
- Class: Marksman
- Attack Type: Ranged
- Max Health: 850
- Max Mana: 450
- Attack Damage: 70
- Move Speed: 5.0
- Attack Range: 10
- Armor: 25
- Magic Resist: 25

## Step 4: Create Skill Data Assets

For each hero, create 3 skills:

**Right-click → Create → MOBA → Skill Data**

### Example Skill Setup (repeat for each hero):

**Normal Skill 1:**
- Name: "Power Strike"
- Type: NormalSkill
- Cooldown: 5
- Mana Cost: 50
- Damage: 100
- Range: 5
- Area of Effect: 3

**Normal Skill 2:**
- Name: "Dash"
- Type: NormalSkill
- Cooldown: 8
- Mana Cost: 60
- Damage: 80
- Range: 7
- Area of Effect: 2

**Ultimate:**
- Name: "Devastation"
- Type: Ultimate
- Cooldown: 60
- Mana Cost: 150
- Damage: 300
- Range: 10
- Area of Effect: 5

## Step 5: Create Passive Data Assets

**Right-click → Create → MOBA → Passive Data**

Create passives for each hero:
- Tank: Bonus Armor (+20)
- Fighter: Bonus Attack Damage (+15)
- Assassin: Bonus Move Speed (+1.0)
- Mage: Bonus Magic Resist (+15)
- Marksman: Bonus Attack Speed (+0.2)

## Step 6: Create Item Data Assets

**Right-click → Create → MOBA → Item Data**

Create 10 items:

1. **Long Sword**
   - Type: PhysicalAttack
   - Attack Damage: 25
   - Cost: 300

2. **Magic Wand**
   - Type: MagicPower
   - Magic Power: 40
   - Cost: 350

3. **Chain Armor**
   - Type: Defense
   - Armor: 30
   - Cost: 400

4. **Magic Cloak**
   - Type: MagicResist
   - Magic Resist: 25
   - Cost: 350

5. **Dagger**
   - Type: AttackSpeed
   - Attack Speed: 0.3
   - Cost: 500

6. **Critical Blade**
   - Type: CriticalChance
   - Critical Chance: 20
   - Cost: 800

7. **Vampiric Blade**
   - Type: Lifesteal
   - Lifesteal: 15
   - Cost: 700

8. **Boots of Speed**
   - Type: MovementSpeed
   - Move Speed: 1.5
   - Cost: 300

9. **Ruby Crystal**
   - Type: Health
   - Bonus Health: 200
   - Cost: 400

10. **Sapphire Crystal**
    - Type: Mana
    - Bonus Mana: 150
    - Cost: 350

## Step 7: Create Prefabs

### Minion Prefab
1. Create → 3D Object → Capsule
2. Scale: (0.5, 0.5, 0.5)
3. Add Minion.cs script
4. Add NavMeshAgent component
5. Add Capsule Collider
6. Save as `Assets/Prefabs/Minion.prefab`

### Hero Prefab (for each class)
1. Create → 3D Object → Capsule
2. Add Hero.cs script
3. Add CharacterController component
4. Assign HeroData asset
5. Save as `Assets/Prefabs/Hero_[ClassName].prefab`

## Step 8: Setup Game Scene

### Create Map Layout

1. **Ground:**
   - Plane at (0, 0, 0), scale (20, 1, 20)
   - Material: Green-ish color

2. **Blue Team (Team 0):**
   - Base Crystal at (-50, 2.5, -50)
   - Top Lane Towers at: (-40, 0, 10), (-30, 0, 15), (-20, 0, 20)
   - Mid Lane Towers at: (-40, 0, -40), (-30, 0, -30), (-20, 0, -20)
   - Bot Lane Towers at: (10, 0, -40), (15, 0, -30), (20, 0, -20)

3. **Red Team (Team 1):**
   - Base Crystal at (50, 2.5, 50)
   - Top Lane Towers at: (40, 0, -10), (30, 0, -15), (20, 0, -20)
   - Mid Lane Towers at: (40, 0, 40), (30, 0, 30), (20, 0, 20)
   - Bot Lane Towers at: (-10, 0, 40), (-15, 0, 30), (-20, 0, 20)

4. **Minion Spawners:**
   - Blue: (-50, 0, 0), (-50, 0, -50), (0, 0, -50)
   - Red: (50, 0, 0), (50, 0, 50), (0, 0, 50)

### Create Waypoints for Minion Paths

For each lane, create empty GameObjects as waypoints:
- Blue_Top_Lane: Start (-50, 0, 0) → ... → End (50, 0, 0)
- Blue_Mid_Lane: Start (-50, 0, -50) → ... → End (50, 0, 50)
- Blue_Bot_Lane: Start (0, 0, -50) → ... → End (0, 0, 50)

Assign waypoint arrays to MinionSpawner.cs scripts.

### Bake NavMesh

1. Window → AI → Navigation
2. Select all walkable surfaces
3. Mark as "Walkable"
4. Click "Bake"

## Step 9: Setup Virtual Joystick UI

In GameScene:

1. Create UI → Canvas (if not exists)
2. Add UI → Image (name it "Joystick Background")
   - Position: Bottom-left corner
   - Size: 150x150
   - Color: Semi-transparent gray

3. Add child UI → Image (name it "Joystick Handle")
   - Size: 50x50
   - Color: White

4. Add VirtualJoystick.cs to Background
5. Assign Background and Handle references

## Step 10: Wire Up All References

In each scene, ensure all script components have their references assigned:

- **LoginScreen**: username input, login button, status text
- **LobbyScreen**: welcome text, buttons
- **HeroSelectionScreen**: hero data array, UI containers
- **GameHUD**: player hero, UI elements (health bar, mana bar, etc.)
- **PlayerController**: hero reference, joystick reference
- **ShopUI**: item data array, hero reference
- **ChatUI**: UI references

## Step 11: Build Settings

1. File → Build Settings
2. Add all scenes in order:
   - LoginScene
   - LobbyScene
   - HeroSelectionScene
   - GameScene
   - ResultScene

3. Switch Platform to Android
4. Player Settings:
   - Company Name: "DeepFlow"
   - Product Name: "MobaGame"
   - Bundle Identifier: "com.deepflow.mobagame"
   - Minimum API Level: 22

## Step 12: Test in Editor

1. Open LoginScene
2. Press Play
3. Test the flow:
   - Login → Lobby → Hero Selection → Game → Results

### Common Issues:

**Scenes not loading:**
- Check Build Settings includes all scenes
- Verify scene names match exactly

**NullReferenceException:**
- Check all script references are assigned
- Verify NavMesh is baked

**Heroes not moving:**
- Ensure CharacterController is attached
- Check NavMeshAgent for minions

**UI not responding:**
- Verify EventSystem exists in scene
- Check Canvas is set to Screen Space - Overlay

## Step 13: Testing Multiplayer (Optional)

1. Start the .NET server:
   ```bash
   cd MobaGame/DotNetServer/MobaServer
   dotnet run
   ```

2. In Unity, modify `TCPClient.cs` serverIP if needed
3. Test chat functionality in-game

## Final Checklist

- [ ] All 5 scenes created and in Build Settings
- [ ] 5 Hero Data assets created with skills and passives
- [ ] 10 Item Data assets created
- [ ] Minion and Hero prefabs created
- [ ] Game scene map layout complete
- [ ] NavMesh baked
- [ ] All UI references wired up
- [ ] Virtual joystick functional
- [ ] Game tested end-to-end in Editor
- [ ] Server tested and connectable

## Performance Optimization

- Quality Settings: Set to "Medium" for mobile
- Target Frame Rate: 30 FPS
- Disable V-Sync for mobile builds
- Use static batching where possible

---

After completing these steps, your Unity MOBA MVP will be fully functional and ready to play!
