# VALORANT ESSENTIALS LAUNCHER

# Elevate to admin and restart if needed
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Start-Process PowerShell -Verb RunAs -ArgumentList "-NoExit", "-File", "`"$PSCommandPath`"", "-ExecutionPolicy", "Bypass"
    exit
}

# Load required assemblies and configure security
Add-Type -AssemblyName System.Windows.Forms; Add-Type -AssemblyName System.Drawing
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

# Global variables and core functions
$script:logBox = $null
$script:basePath = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$script:qresPath = Join-Path $script:basePath "QRes.exe"
$script:configPath = Join-Path $script:basePath "config.json"
$script:nativeResolution = $null
$script:valorantPaksPath = $null # Will be populated once at startup

function Add-Log { param ([string]$Message, [string]$Color = "White"); if ($null -ne $script:logBox) { $script:logBox.Invoke([Action]{ $script:logBox.SelectionStart = $script:logBox.TextLength; $script:logBox.SelectionLength = 0; $script:logBox.SelectionColor = $Color; $script:logBox.AppendText("$(Get-Date -Format 'HH:mm:ss') - $Message`r`n"); $script:logBox.ScrollToCaret() }) } }

# Locate and store Valorant Paks path
function Set-GlobalValorantPaksPath {
    # 1. Check for a saved, valid path first.
    if (Test-Path $script:configPath) {
        $savedConfig = Get-Content $script:configPath | ConvertFrom-Json
        if ($savedConfig.ValorantPaksPath -and (Test-Path $savedConfig.ValorantPaksPath)) {
            Add-Log "Found saved Valorant path."
            $script:valorantPaksPath = $savedConfig.ValorantPaksPath
            return
        }
    }

    # 2. If no valid saved path, try the registry.
    Add-Log "No saved path. Checking registry..."
    $regPaths = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Riot Game valorant.live", "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Riot Game valorant.live"
    foreach ($regKey in $regPaths) {
        if (Test-Path $regKey) {
            $installLocation = (Get-ItemProperty -Path $regKey).InstallLocation
            $paksPathCandidate = Join-Path $installLocation "live\ShooterGame\Content\Paks"
            if ($installLocation -and (Test-Path $paksPathCandidate)) {
                Add-Log "Found Valorant in registry. Saving for future use."
                $script:valorantPaksPath = $paksPathCandidate
                @{ ValorantPaksPath = $script:valorantPaksPath } | ConvertTo-Json | Set-Content $script:configPath
                return
            }
        }
    }

    # 3. If registry fails, ask the user.
    Add-Log "Could not find path automatically. Please locate it manually." "Yellow"
    $folderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog
    $folderBrowser.Description = "Please select your VALORANT 'live' folder (e.g., C:\Riot Games\VALORANT\live)"
    if ($folderBrowser.ShowDialog() -eq "OK") {
        $paksPathCandidate = Join-Path $folderBrowser.SelectedPath "ShooterGame\Content\Paks"
        if (Test-Path $paksPathCandidate) {
            Add-Log "Valorant path confirmed. Saving for future use."
            $script:valorantPaksPath = $paksPathCandidate
            @{ ValorantPaksPath = $script:valorantPaksPath } | ConvertTo-Json | Set-Content $script:configPath
            return
        } else { throw "Invalid folder selected. The 'ShooterGame\Content\Paks' subfolder was not found. Please try again." }
    } else { throw "Operation cancelled by user." }
}
function Process-IniFile { param ([string]$FilePath, [string]$Width, [string]$Height); if (-not (Test-Path $FilePath)) { return }; Add-Log "Processing: $($FilePath.Split('\')[-3..-1] -join '\')"; $fileObject = Get-Item $FilePath; $wasReadOnly = $fileObject.IsReadOnly; try { if ($wasReadOnly) { $fileObject.IsReadOnly = $false }; $newContent = Get-Content $FilePath | ForEach-Object { switch -Wildcard ($_){ 'ResolutionSizeX=*'{ "ResolutionSizeX=$Width" } 'ResolutionSizeY=*'{ "ResolutionSizeY=$Height" } 'LastUserConfirmedResolutionSizeX=*'{ "LastUserConfirmedResolutionSizeX=$Width" } 'LastUserConfirmedResolutionSizeY=*'{ "LastUserConfirmedResolutionSizeY=$Height" } 'DesiredScreenWidth=*'{ "DesiredScreenWidth=$Width" } 'DesiredScreenHeight=*'{ "DesiredScreenHeight=$Height" } 'LastUserConfirmedDesiredScreenWidth=*'{ "LastUserConfirmedDesiredScreenWidth=$Width" } 'LastUserConfirmedDesiredScreenHeight='{ "LastUserConfirmedDesiredScreenHeight=$Height" } 'bShouldLetterbox=*'{ "bShouldLetterbox=False" } 'LastConfirmedShouldLetterbox=*'{ "LastConfirmedShouldLetterbox=False" } 'bUseVSync=*'{ "bUseVSync=False" } 'bUseDynamicResolution=*'{ "bUseDynamicResolution=False" } 'LastConfirmedFullscreenMode=*'{ "LastConfirmedFullscreenMode=2" } 'PreferredFullscreenMode=*'{ "PreferredFullscreenMode=2" } 'FullscreenMode=*'{ "FullscreenMode=2" } default { $_ } } }; Set-Content -Path $FilePath -Value $newContent; } finally { if ($wasReadOnly) { (Get-Item $FilePath).IsReadOnly = $true } } }

# --- Modern GUI Setup ---
# Colors
$accentColor = [System.Drawing.Color]::FromArgb(114, 137, 218)
$successColor = [System.Drawing.Color]::FromArgb(67, 181, 129)
$textColor = [System.Drawing.Color]::White
$darkBg = [System.Drawing.Color]::FromArgb(42, 45, 62)
$darkerBg = [System.Drawing.Color]::FromArgb(35, 37, 56)

# Form setup with modern styling
$mainForm = New-Object System.Windows.Forms.Form
$mainForm.Text = "ValorantEssentials"
$mainForm.Size = New-Object System.Drawing.Size(500, 600)
$mainForm.StartPosition = "CenterScreen"
$mainForm.FormBorderStyle = "FixedDialog"
$mainForm.MaximizeBox = $false
$mainForm.BackColor = [System.Drawing.Color]::FromArgb(30, 30, 46)
$mainForm.Padding = New-Object System.Windows.Forms.Padding(20)

# Fonts
$font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Regular)
$titleFont = New-Object System.Drawing.Font("Segoe UI", 12, [System.Drawing.FontStyle]::Bold)
$mainForm.Font = $font

# Function to create styled buttons
function New-StyledButton {
    param(
        [string]$Text,
        [int]$X,
        [int]$Y,
        [int]$Width,
        [int]$Height,
        [System.Drawing.Color]$BackColor
    )
    
    $button = New-Object System.Windows.Forms.Button
    $button.Text = $Text
    $button.Location = New-Object System.Drawing.Point($X, $Y)
    $button.Size = New-Object System.Drawing.Size($Width, $Height)
    $button.BackColor = $BackColor
    $button.ForeColor = $textColor
    $button.Font = $font
    $button.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $button.FlatAppearance.BorderSize = 0
    $button.Cursor = [System.Windows.Forms.Cursors]::Hand
    $button.FlatAppearance.MouseOverBackColor = [System.Drawing.Color]::FromArgb(
        [Math]::Min($BackColor.R + 30, 255),
        [Math]::Min($BackColor.G + 30, 255),
        [Math]::Min($BackColor.B + 30, 255)
    )
    
    return $button
}

# Function to create input fields
function New-InputField {
    param(
        [string]$Text,
        [int]$X,
        [int]$Y,
        [int]$Width,
        [string]$Value
    )
    
    $textBox = New-Object System.Windows.Forms.TextBox
    $textBox.Text = $Value
    $textBox.Location = New-Object System.Drawing.Point($X, $Y)
    $textBox.Size = New-Object System.Drawing.Size($Width, 35)
    $textBox.BackColor = $darkerBg
    $textBox.ForeColor = $textColor
    $textBox.BorderStyle = [System.Windows.Forms.BorderStyle]::FixedSingle
    
    $label = New-Object System.Windows.Forms.Label
    $label.Text = $Text
    $label.Location = New-Object System.Drawing.Point($X, ($Y - 25))
    $label.Size = New-Object System.Drawing.Size($Width, 20)
    $label.ForeColor = $textColor
    
    return @($textBox, $label)
}

# Header
$headerLabel = New-Object System.Windows.Forms.Label
$headerLabel.Text = "VALORANT ESSENTIALS"
$headerLabel.Location = New-Object System.Drawing.Point(20, 20)
$headerLabel.Size = New-Object System.Drawing.Size(440, 30)
$headerLabel.Font = $titleFont
$headerLabel.ForeColor = $accentColor

# Main buttons
$updatePaksButton = New-StyledButton -Text "INSTALL / UPDATE BLOOD PAKS" -X 20 -Y 70 -Width 440 -Height 45 -BackColor $accentColor

# Resolution Group
$stretchGroup = New-Object System.Windows.Forms.GroupBox
$stretchGroup.Location = New-Object System.Drawing.Point(20, 135)
$stretchGroup.Size = New-Object System.Drawing.Size(440, 200)
$stretchGroup.Text = "STRETCHED RESOLUTION"
$stretchGroup.ForeColor = $textColor
$stretchGroup.BackColor = $darkBg
$stretchGroup.Font = $font

# Resolution inputs
$inputFields = New-InputField -Text "WIDTH" -X 20 -Y 60 -Width 180 -Value "1440"
$widthTextBox = $inputFields[0]
$widthLabel = $inputFields[1]

$inputFields = New-InputField -Text "HEIGHT" -X 220 -Y 60 -Width 180 -Value "1080"
$heightTextBox = $inputFields[0]
$heightLabel = $inputFields[1]

# Start button
$startLauncherButton = New-StyledButton -Text "START STRETCHED RESOLUTION" -X 20 -Y 120 -Width 400 -Height 45 -BackColor $successColor

# Log section
$logLabel = New-Object System.Windows.Forms.Label
$logLabel.Text = "STATUS LOG"
$logLabel.Location = New-Object System.Drawing.Point(20, 355)
$logLabel.Size = New-Object System.Drawing.Size(440, 25)
$logLabel.ForeColor = $textColor
$logLabel.Font = $titleFont

# Log box with modern styling
$logBox = New-Object System.Windows.Forms.RichTextBox
$logBox.Location = New-Object System.Drawing.Point(20, 390)
$logBox.Size = New-Object System.Drawing.Size(440, 150)
$logBox.ReadOnly = $true
$logBox.BackColor = $darkerBg
$logBox.ForeColor = $textColor
$logBox.Font = New-Object System.Drawing.Font("Consolas", 9)
$logBox.BorderStyle = [System.Windows.Forms.BorderStyle]::FixedSingle
$script:logBox = $logBox

# Add controls to group
$stretchGroup.Controls.Add($widthLabel)
$stretchGroup.Controls.Add($widthTextBox)
$stretchGroup.Controls.Add($heightLabel)
$stretchGroup.Controls.Add($heightTextBox)
$stretchGroup.Controls.Add($startLauncherButton)

# Add all controls to form
$mainForm.Controls.Add($headerLabel)
$mainForm.Controls.Add($updatePaksButton)
$mainForm.Controls.Add($stretchGroup)
$mainForm.Controls.Add($logLabel)
$mainForm.Controls.Add($logBox)

# --- Button Click Logic & Monitoring Timer ---
$script:isStretched = $false
$monitoringTimer = New-Object System.Windows.Forms.Timer; $monitoringTimer.Interval = 5000
$monitoringTimer.Add_Tick({ $valorantIsRunning = (Get-Process -Name "VALORANT-Win64-Shipping" -ErrorAction SilentlyContinue) -ne $null; if ($valorantIsRunning -and -not $script:isStretched) { Add-Log "Valorant detected! Switching resolution..." "Magenta"; Start-Process -FilePath $script:qresPath -ArgumentList "/x:$($widthTextBox.Text) /y:$($heightTextBox.Text)" -NoNewWindow; $script:isStretched = $true } elseif (-not $valorantIsRunning -and $script:isStretched) { Add-Log "Valorant closed. Reverting resolution..." "Magenta"; Start-Process -FilePath $script:qresPath -ArgumentList "/x:$($script:nativeResolution.Width) /y:$($script:nativeResolution.Height)" -NoNewWindow; $script:isStretched = $false } })
$updatePaksButton.Add_Click({
    $mainForm.Cursor = "WaitCursor"; $updatePaksButton.Enabled = $false; $startLauncherButton.Enabled = $false
    try {
        Add-Log "Starting 'Blood Paks' update..."
        if (-not $script:valorantPaksPath) { throw "Valorant path is not set. Could not complete operation." }
        
        Add-Log "Deleting old VNG files..."; Remove-Item -Path (Join-Path $script:valorantPaksPath "VNGLogo-WindowsClient.*") -Force -ErrorAction SilentlyContinue
        
        Add-Log "Downloading new MatureData files..."
        $repoBaseUrl = "https://raw.githubusercontent.com/jianonrepeat/ValorantEssentials/main/paks"; $tempDownloadPath = Join-Path $env:TEMP "ValorantPaksDownload_$(Get-Random)"; New-Item -Path $tempDownloadPath -ItemType Directory -Force | Out-Null
        $filesToDownload = "MatureData-WindowsClient.pak", "MatureData-WindowsClient.sig", "MatureData-WindowsClient.ucas", "MatureData-WindowsClient.utoc"
        foreach ($file in $filesToDownload) { Invoke-WebRequest -Uri "$repoBaseUrl/$file" -OutFile (Join-Path $tempDownloadPath $file) }

        Add-Log "Copying files to Valorant..."; Copy-Item -Path "$tempDownloadPath\*" -Destination $script:valorantPaksPath -Force
        Remove-Item -Path $tempDownloadPath -Recurse -Force
        
        Add-Log "Update complete!" "LimeGreen"
    } catch { Add-Log "[ERROR] $($_.Exception.Message)" "Red" }
    finally { $mainForm.Cursor = "Default"; $updatePaksButton.Enabled = $true; $startLauncherButton.Enabled = $true }
})
$startLauncherButton.Add_Click({
    $mainForm.Cursor = "WaitCursor"; $updatePaksButton.Enabled = $false; $startLauncherButton.Enabled = $false
    try {
        Add-Log "Starting Stretched Res setup..."; $stretchedWidth = $widthTextBox.Text; $stretchedHeight = $heightTextBox.Text
        $configBasePath = Join-Path $env:USERPROFILE "AppData\Local\VALORANT\Saved\Config"; if (-not (Test-Path $configBasePath)) { throw "Valorant config directory not found." }; $configFolders = Get-ChildItem -Path $configBasePath -Directory | Where-Object { $_.Name -ne "CrashReportClient" }; if ($configFolders.Count -eq 0) { throw "No config folders found. Launch Valorant to main menu once." }
        Add-Log "Patching config files..."; foreach ($folder in $configFolders) { $iniFilePath = if ($folder.Name -eq "WindowsClient") { Join-Path $folder.FullName "GameUserSettings.ini" } else { Join-Path $folder.FullName "WindowsClient\GameUserSettings.ini" }; Process-IniFile -FilePath $iniFilePath -Width $stretchedWidth -Height $stretchedHeight }
        $monitoringTimer.Start()
        Add-Log "Setup complete! Monitoring started." "LimeGreen"; Add-Log "KEEP THIS APP OPEN WHILE YOU PLAY.", "Yellow"
    } catch { Add-Log "[ERROR] $($_.Exception.Message)" "Red"; $updatePaksButton.Enabled = $true; $startLauncherButton.Enabled = $true }
    finally { $mainForm.Cursor = "Default" }
})
$mainForm.Add_FormClosing({ $monitoringTimer.Stop() })

# --- SCRIPT ENTRY POINT ---
$mainForm.Add_Load({
    try {
        Add-Log "Initializing..."
        $script:nativeResolution = @{ Width = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Width; Height = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Height }
        Add-Log "Native resolution captured: $($script:nativeResolution.Width)x$($script:nativeResolution.Height)"
        if (-not (Test-Path $script:qresPath)) { Add-Log "Resolution tool not found. Downloading..." "Yellow"; Invoke-WebRequest -Uri "https://github.com/jianonrepeat/ScreenResolutionChanger/raw/refs/heads/master/QRes.exe" -OutFile $script:qresPath; if (-not (Test-Path $script:qresPath)) { throw "Download failed." }; Add-Log "Download complete." "LimeGreen" } else { Add-Log "Resolution tool (QRes.exe) found." }
        
        # This now runs ONCE at startup to get the path ready for all other functions.
        Set-GlobalValorantPaksPath

    } catch { Add-Log "[ERROR] Could not initialize: $($_.Exception.Message)" "Red" }
})

[void]$mainForm.ShowDialog()