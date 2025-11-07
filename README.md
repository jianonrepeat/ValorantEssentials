# ValorantEssentials: Your All-in-One VALORANT Utility

A powerful PowerShell-based tool for Windows that enhances your VALORANT experience with custom stretched resolution and easy content management.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![PowerShell: 5.1+](https://img.shields.io/badge/PowerShell-5.1+-blue.svg)](https://docs.microsoft.com/en-us/powershell/)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows-0078D6.svg)](https://www.microsoft.com/windows)

---

## Why Use ValorantEssentials?

ValorantEssentials is designed for players who want to optimize their gameplay and customize their experience without hassle. Whether you're looking to gain a competitive edge with a custom aspect ratio or install unique visual assets, this tool automates the process safely and efficiently.

### Key Features

- **Gain a Competitive Edge**: Apply stretched resolutions like `1440x1080` to widen targets and potentially improve your aim.
- **Automated Setup**: The tool automatically detects your VALORANT installation and handles resolution changes for you.
- **Custom Content**: Easily install and manage custom `.pak` files, such as the popular "Blood Paks," to modify in-game visuals.
- **User-Friendly Interface**: A simple, dark-themed GUI makes it easy to access all features.
- **Safe to Use**: Modifies configuration files only and does not interact with Vanguard anti-cheat or game memory.

## Getting Started

### Prerequisites

- Windows 10 or 11 (64-bit)
- VALORANT installed
- Administrator privileges

### Installation & First Use

1.  **Download**: Grab the latest version from the [**Releases Page**](https://github.com/yourusername/ValorantEssentials/releases).
2.  **Extract**: Unzip the downloaded file into a folder of your choice.
3.  **Run**: Right-click `run.bat` and select **Run as administrator**.

## How to Use

### Applying Stretched Resolution

1.  Launch the application as an administrator.
2.  Enter your desired resolution in the input box (e.g., `1440x1080`).
3.  Click the **Start Stretched Res Launcher** button.
4.  Launch VALORANT. The script will automatically apply the custom resolution when the game starts and revert it when you exit.

### Installing Custom Content

1.  Ensure VALORANT is completely closed.
2.  Click the **Install/Update 'Blood Paks'** button.
3.  The tool will download and install the necessary files into your game directory.
4.  Restart VALORANT to see the changes.

## Configuration (`config.json`)

The `config.json` file allows for manual overrides if auto-detection fails.

-   `ValorantPaksPath`: The full path to your VALORANT `Paks` directory.
    -   *Example*: `"C:\\Riot Games\\VALORANT\\live\\ShooterGame\\Content\\Paks"`
-   `QResUrl`: The download URL for `QRes.exe`, the utility that manages resolution switching.
-   `PaksRepoUrl`: The repository URL where custom content is hosted.

## Troubleshooting

-   **Permission Denied?** The script requires administrator rights to modify game files and change screen resolution. Please ensure you run `run.bat` as an administrator.
-   **Resolution Not Working?** In VALORANT's video settings, ensure the display mode is set to **Windowed Fullscreen**.
-   **Custom Content Not Loading?** Verify that the `ValorantPaksPath` in `config.json` is correct and that the game was fully closed before you tried installing.

## A Note on Safety

This tool is designed with safety in mind. It does not hook into the game's process, read game memory, or interact with Vanguard anti-cheat in any way. It only automates file management and resolution changes that could otherwise be done manually. However, the use of custom game files is done at your own risk.

## How to Contribute

Contributions are welcome! If you have an idea for a new feature or a bug fix, please fork the repository, make your changes, and submit a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

***

*Disclaimer: This project is not affiliated with, authorized, or endorsed by Riot Games, Inc. All game content and materials are trademarks and copyrights of their respective owners.*