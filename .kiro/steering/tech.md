# Technology Stack

## Engine & Version
- Unity 6000.1.0f1 (Unity 6)
- Universal Render Pipeline (URP) 17.1.0

## Key Dependencies
- **Addressables** (2.4.6) - Asset management and loading
- **Input System** (1.14.0) - Modern input handling
- **TextMesh Pro** (2.0.0) - Advanced text rendering
- **Visual Scripting** (1.9.6) - Node-based scripting
- **Timeline** (1.8.7) - Cinematic and animation sequencing
- **Multiplayer Center** (1.0.0) - Networking foundation
- **unity.libsodium** - Encryption library for secure data handling

## Platform Support
- Mobile-focused (com.unity.feature.mobile)
- 2D features enabled (com.unity.feature.2d)

## Development Tools
- Rider IDE support (3.0.36)
- Visual Studio support (2.0.23)
- Collab Proxy (2.8.2) for version control

## Common Commands
Since this is a Unity project, development is primarily done through the Unity Editor:

- **Build**: Use Unity Editor Build Settings or Build & Run
- **Testing**: Unity Test Runner (Window > General > Test Runner)
- **Package Management**: Window > Package Manager
- **Addressables**: Window > Asset Management > Addressables > Groups

## Architecture Notes
- Uses singleton pattern for managers (PurchaseHistoryManager, PurchaseHistorySender)
- Coroutine-based async operations for network requests
- Component-based architecture following Unity patterns