# Valorant Essentials
A C# Windows Forms tool for managing Valorant-specific enhancements: Blood Paks installation/updates and stretched resolution configuration.

## Purpose
Simplifies two key Valorant customizations:
1. Installing/updating Blood Paks (custom game assets)
2. Setting up and maintaining stretched resolution for gameplay

## Key Features
- Auto-detects Valorant's installation path via registry (falls back to manual selection)
- Downloads missing `QRes.exe` (resolution switching tool) automatically
- Patches Valorant's `GameUserSettings.ini` files for stretched resolution
- Monitors Valorant process to switch resolution on launch/exit
- Admin rights enforcement for critical file operations
- Real-time status logging in the GUI
- Persistent configuration storage in `config.json`
- Modern, responsive UI with progress indicators
- Comprehensive error handling and logging

## Prerequisites
- Windows operating system
- .NET 8.0 Runtime or SDK
- Valorant installed (launch at least once to generate config files)
- Internet connection (for initial `QRes.exe` download and Blood Paks updates)

## Installation
1. Clone or download the ValorantEssentials repository to your local machine
2. Build the project using .NET CLI or Visual Studio:
   ```bash
   dotnet build -c Release
   ```
3. Run the application:
   ```bash
   dotnet run
   ```
   Or run the executable directly from `bin/Release/net8.0-windows/`

## Usage

### Blood Paks Installation
1. Launch Valorant Essentials
2. Click "INSTALL / UPDATE BLOOD PAKS"
3. The tool will automatically download and install the latest Blood Paks
4. Files will be copied to your Valorant Paks directory

### Stretched Resolution Setup
1. Enter your desired resolution width and height
2. Click "START STRETCHED RESOLUTION"
3. The tool will patch your Valorant config files
4. Keep the application open while playing (it monitors Valorant process)
5. Resolution will automatically switch when Valorant launches/exits

## Project Structure
```
ValorantEssentials/
├── Models/                    # Data models and configuration
│   └── AppConfiguration.cs   # Application settings management
├── Services/                 # Business logic and service layer
│   ├── ServiceManager.cs   # Dependency injection container
│   └── ValidationService.cs # Input validation
├── Utilities/               # Helper classes and utilities
│   ├── FileDownloader.cs   # HTTP file download with progress
│   ├── IniFileHelper.cs    # INI file parsing and modification
│   ├── Logger.cs           # Comprehensive logging system
│   ├── ProcessMonitor.cs   # Valorant process monitoring
│   ├── RegistryHelper.cs   # Windows registry operations
│   └── ResolutionHelper.cs # Display resolution management
├── MainForm.cs             # Main Windows Forms UI
├── Program.cs              # Application entry point
├── ValorantEssentials.csproj # Project configuration
└── config.json             # User configuration (created at runtime)
```

## Technical Details

### Architecture
- **Service-Oriented Design**: Clean separation of concerns with interfaces
- **Dependency Injection**: Centralized service management
- **Async/Await**: Non-blocking operations for UI responsiveness
- **Event-Driven**: Real-time logging and process monitoring
- **Error Handling**: Comprehensive exception handling and user feedback

### Key Components
- **FileDownloader**: Optimized HTTP downloads with progress reporting and cancellation
- **ProcessMonitor**: Real-time Valorant process detection using threading timers
- **ResolutionService**: Safe display resolution switching with QRes.exe
- **IniFileService**: Robust INI file parsing and modification
- **Logger**: Thread-safe logging with multiple log levels and event notifications

## Configuration
The application creates a `config.json` file in the executable directory to store:
- Valorant installation path
- User preferences
- Application settings

## Safety Features
- Automatic backup and restore of native resolution
- Safe file operations with read-only attribute management
- Process monitoring prevents resolution issues
- Comprehensive error handling prevents system issues

## Building from Source

### Requirements
- .NET 8.0 SDK or later
- Windows operating system
- Visual Studio 2022 or VS Code (optional)

### Build Commands
```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Publish self-contained executable
dotnet publish -c Release -r win-x64 --self-contained true
```

## Contributing
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License
This project is licensed under the MIT License - see the LICENSE file for details.

## Disclaimer
This tool is not affiliated with Riot Games or Valorant. Use at your own risk. Always backup your game files before making modifications.

## Support
For issues and feature requests, please use the GitHub issue tracker.