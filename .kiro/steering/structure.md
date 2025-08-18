# Project Structure

## Root Organization
```
Assets/
├── Scripts/           # Core C# scripts and game logic
├── Scenes/           # Unity scene files
├── Settings/         # Render pipeline and project settings
├── AddressableAssetsData/  # Addressable asset configuration
├── TextMesh Pro/     # TextMesh Pro fonts and resources
├── UI Toolkit/       # UI Toolkit themes and styles
├── test/            # Development testing files and prototypes
├── tt/              # Additional testing/temporary content
└── unity.libsodium/ # Encryption library integration
```

## Key Directories

### Scripts/
Core application logic including:
- `GameInitializer.cs` - Main game startup and initialization
- `JsonFetcher.cs` - Network data retrieval
- `PurchaseHistoryManager.cs` - Purchase tracking singleton
- `PurchaseHistorySender.cs` - Purchase data transmission
- `Decryptor.cs` & `SimpleDecryptor.cs` - Encryption handling

### Scenes/
- `SampleScene.unity` - Main scene file

### Settings/
- URP configuration files
- Renderer settings for 2D pipeline
- Scene templates

### test/
Development and testing assets:
- Prototype scripts and prefabs
- Test data files (JSON, encrypted data)
- UI Toolkit experimental files
- Python encryption utilities

## Naming Conventions
- **Scripts**: PascalCase for classes and public members
- **Scenes**: Descriptive names (e.g., SampleScene)
- **Prefabs**: Descriptive names matching their purpose
- **Assets**: Organized by type and functionality

## Architecture Patterns
- **Singleton Pattern**: Used for managers (PurchaseHistoryManager, PurchaseHistorySender)
- **Component-Based**: Standard Unity MonoBehaviour components
- **Separation of Concerns**: Network, encryption, and game logic separated into distinct scripts