# Valorant Essentials
A PowerShell GUI tool for managing Valorant-specific enhancements: Blood Paks installation/updates and stretched resolution configuration.

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

## Prerequisites
- Windows operating system
- Valorant installed (launch at least once to generate config files)
- PowerShell 5.1 or later (pre-installed on Windows 10+)
- Internet connection (for initial `QRes.exe` download and Blood Paks updates)

## Installation
1. Clone or download the ValorantEssentials repository to your local machine
2. Ensure the project directory structure is intact:
   - `ValorantEssentials.ps1` (main tool)
   - `config.json` (configuration file)
   - `run.bat` (launcher shortcut)

## Usage Instructions
### 1. Install/Update Blood Paks
- Launch `ValorantEssentials.ps1` (runs as admin automatically if not already)
- Click the **INSTALL / UPDATE BLOOD PAKS** button
- The tool will delete old VNG files, download new MatureData files, and copy them to Valorant's Paks directory

### 2. Set Up Stretched Resolution
- Enter your desired stretched resolution (default: 1728x1080) in the WIDTH/HEIGHT fields
- Click **START STRETCHED RESOLUTION**
- **IMPORTANT:** After clicking this button, *then* launch Valorant.
- The tool will patch Valorant's config files and start monitoring the Valorant process
- Keep the tool open while playing (resolution will revert when Valorant closes or the tool is exited)

## Configuration (`config.json`)
- `ValorantPaksPath`: Auto-saved path to Valorant's `live/ShooterGame/Content/Paks` directory
- `QResUrl`: Download URL for the `QRes.exe` resolution tool
- `PaksRepoUrl`: Source URL for Blood Paks files

## Troubleshooting
- **Valorant path not found**: Manually select the `VALORANT/live` folder when prompted
- **Config files missing**: Launch Valorant to the main menu once, then close the game to generate config files.
- **Resolution not switching**: Ensure `QRes.exe` was downloaded successfully (check project directory)

## Technical Details
- **Admin Privileges**: The script requires administrator privileges to modify system settings and Valorant game files. It will prompt for elevation if not run as admin.
- **Resolution Switching**: The tool utilizes `QRes.exe` (downloaded automatically if missing) to change screen resolutions. This is a common command-line utility for display settings.
- **Config File Patching**: The script modifies `GameUserSettings.ini` files within your Valorant configuration directory to apply stretched resolution settings. It handles read-only attributes to ensure changes can be applied.
- **Process Monitoring**: A background timer continuously checks for the Valorant game process. When detected, it applies the stretched resolution; when Valorant closes, it reverts to the native resolution.

## Notes
- Always keep the tool open while playing Valorant for resolution management
- The tool reverts to your native resolution when Valorant closes or the tool is exited

## License
This project is open-source and available under the [MIT License](LICENSE).

## Contributing
Contributions are welcome! If you have suggestions for improvements, bug fixes, or new features, please feel free to open an issue or submit a pull request on the GitHub repository.