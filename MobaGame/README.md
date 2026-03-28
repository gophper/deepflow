# MOBA Game - Unity Client & .NET Server

This is a Unity 2022.3 LTS MOBA MVP prototype with a .NET TCP server backend.

## Project Structure

```
MobaGame/
├── UnityClient/          # Unity 2022.3 LTS client project
│   ├── Assets/
│   │   ├── Scenes/      # Game scenes
│   │   ├── Scripts/     # C# game scripts
│   │   ├── Prefabs/     # Game prefabs
│   │   └── Resources/   # Resources (hero data, items, etc.)
│   ├── ProjectSettings/ # Unity project settings
│   └── Packages/        # Unity package manager
└── DotNetServer/        # .NET 8.0 TCP server
    └── MobaServer/      # Server console application
```

## Requirements

### Unity Client
- **Unity 2022.3 LTS** or later
- Target Platform: Android (can also run in Unity Editor)
- Minimum Android SDK: API Level 22

### .NET Server
- **.NET 8.0 SDK** or later
- Works on Windows, Linux, and macOS

## Getting Started

### 1. Running the Server

```bash
cd MobaGame/DotNetServer/MobaServer
dotnet run
```

The server will start on port `8888` by default. You should see:
```
=== MOBA Game Server ===
Starting server...

Server started on port 8888
Waiting for connections...
```

Press 'q' to stop the server.

### 2. Running the Unity Client

1. Open Unity Hub
2. Click "Add" and navigate to `MobaGame/UnityClient/`
3. Select the folder and open the project in Unity 2022.3 LTS
4. Wait for Unity to import all assets and compile scripts
5. Open the `LoginScene` scene in `Assets/Scenes/`
6. Click the Play button in Unity Editor

### 3. Playing the Game

**Game Flow:**
1. **Login Screen**: Enter any username (placeholder authentication)
2. **Lobby**: Click "Quick Match" to start matchmaking
3. **Hero Selection**: Choose one of 5 hero classes
4. **Battle**: Play the MOBA match
5. **Results**: View match statistics

**Controls (In Editor):**
- **WASD** or **Arrow Keys**: Move hero
- **Virtual Joystick**: (On mobile/touch devices)
- **Q**: Use Skill 1
- **E**: Use Skill 2  
- **R**: Use Ultimate
- **Space**: Basic Attack
- **B**: Open Shop
- **Enter**: Open Chat

**Controls (On Android/Touch):**
- **Virtual Joystick** (bottom-left): Move hero
- **Attack Button**: Basic attack
- **Skill Buttons**: Use skills 1, 2, and ultimate
- **Shop Button**: Open item shop
- **Chat Button**: Open chat

## Game Features

### Core Gameplay
- ✅ 5v5 Three-lane map (Top, Mid, Bot)
- ✅ 3 Towers per lane + Base Crystal
- ✅ Jungle buff spawn points (Red/Blue)
- ✅ 5 Hero classes with unique abilities
- ✅ Minion spawning and AI
- ✅ Tower auto-targeting
- ✅ Crystal destruction victory condition

### Heroes
- **Tank**: High defense, melee attacks
- **Fighter**: Balanced stats, melee attacks
- **Assassin**: High damage, melee attacks
- **Mage**: Magic damage, ranged attacks
- **Marksman**: Physical damage, ranged attacks

Each hero has:
- Basic attack (melee/ranged)
- 2 Normal skills
- 1 Ultimate skill
- 1 Passive ability

### Economy & Progression
- Gold earned from kills and objectives
- 10 basic items available in shop
- Recommended builds per hero class
- Experience and leveling system
- Death and respawn mechanics

### Social Features
- In-game text chat
- End-game scoreboard (K/D/A, gold, towers)
- Real-time match statistics

### Networking (Basic)
- TCP connection to server
- Chat message forwarding
- Player state sync framework
- Matchmaking skeleton

## Building for Android

1. In Unity: File → Build Settings
2. Select "Android" platform
3. Click "Switch Platform"
4. Configure Player Settings:
   - Set Company Name and Product Name
   - Set minimum API level to 22
5. Click "Build" or "Build and Run"

## Network Configuration

**Server IP Configuration:**
- Edit `Assets/Scripts/Network/TCPClient.cs`
- Change `serverIP` field to your server's IP address
- Default: `127.0.0.1` (localhost)

**Server Port:**
- Default port: `8888`
- Can be changed in both server and client code

## Development Notes

### Current Implementation
- Local/single-player mode functional
- Network infrastructure ready for multiplayer
- Primitive placeholder graphics (Capsules, Cubes)
- Basic AI for minions and towers

### Performance Target
- Target: 30 FPS on mobile devices
- Optimized for Android platform
- Lightweight rendering and logic

### Known Limitations (MVP)
- Placeholder art (primitives only)
- Basic matchmaking (fake/local)
- Simplified network sync
- No anti-cheat or security
- Limited UI polish

## Next Steps for Full Implementation

1. **Art Assets**: Replace primitives with actual 3D models
2. **Network Sync**: Implement authoritative server
3. **Matchmaking**: Real matchmaking system
4. **Account System**: Proper authentication
5. **Advanced AI**: Smarter bot players
6. **Polish**: VFX, SFX, animations
7. **Balance**: Hero and item balancing

## Troubleshooting

**Unity won't open the project:**
- Ensure you have Unity 2022.3 LTS installed
- Check Unity Hub for correct version

**Server won't start:**
- Ensure .NET 8.0 SDK is installed: `dotnet --version`
- Check if port 8888 is already in use
- Try running as administrator (Windows)

**Can't connect to server:**
- Verify server is running
- Check firewall settings
- Ensure correct IP address in client code
- For localhost testing, use `127.0.0.1`

**Game crashes or freezes:**
- Check Unity Console for errors
- Ensure all required scenes are in Build Settings
- Verify NavMesh is baked for the game scene

## License

This is an MVP prototype for demonstration purposes.

## Contact

For issues or questions, please refer to the repository issues page.
