# VALORANT ESSENTIALS LAUNCHER

# Elevate to admin and restart if needed
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Start-Process PowerShell -Verb RunAs -ArgumentList "-NoExit", "-File", "`"$PSCommandPath`"", "-ExecutionPolicy", "Bypass"
    exit
}

# Load required assemblies and configure security
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

# Global variables
$script:logBox = $null
$script:basePath = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$script:qresPath = Join-Path $script:basePath "QRes.exe"
$script:configPath = Join-Path $script:basePath "config.json"
$script:nativeResolution = $null
$script:config = $null
$script:isStretched = $false

# --- THEME COLORS ---
$theme = @{
    ValorantRed     = [System.Drawing.Color]::FromArgb(253, 69, 86)
    OffWhite        = [System.Drawing.Color]::FromArgb(250, 250, 250)
    DarkCharcoal    = [System.Drawing.Color]::FromArgb(28, 28, 28)
    DarkerCharcoal  = [System.Drawing.Color]::FromArgb(18, 18, 18)
    LightCharcoal   = [System.Drawing.Color]::FromArgb(58, 58, 58)
    LogSuccess      = [System.Drawing.Color]::FromArgb(50, 205, 50)
    LogError        = [System.Drawing.Color]::FromArgb(255, 69, 58)
    LogInfo         = [System.Drawing.Color]::FromArgb(255, 255, 102)
    LogAction       = [System.Drawing.Color]::FromArgb(200, 160, 255)
}

# --- CORE FUNCTIONS ---

function Add-Log {
    param ([string]$Message, [System.Drawing.Color]$Color = $theme.OffWhite)
    if ($null -ne $script:logBox) {
        $script:logBox.Invoke([Action]{
            $script:logBox.SelectionStart = $script:logBox.TextLength
            $script:logBox.SelectionLength = 0
            $script:logBox.SelectionColor = $Color
            $script:logBox.AppendText("$(Get-Date -Format 'HH:mm:ss') - $Message`r`n")
            $script:logBox.ScrollToCaret()
        })
    }
}

function Load-Configuration {
    if (-not (Test-Path $script:configPath)) {
        $defaultConfig = @{
            ValorantPaksPath = ""
            QResUrl = "https://github.com/jianonrepeat/ScreenResolutionChanger/raw/refs/heads/master/QRes.exe"
            PaksRepoUrl = "https://raw.githubusercontent.com/jianonrepeat/ValorantEssentials/main/paks"
        }
        $defaultConfig | ConvertTo-Json -Depth 3 | Set-Content $script:configPath
    }
    $script:config = Get-Content $script:configPath | ConvertFrom-Json
}

function Save-Configuration {
    $script:config | ConvertTo-Json -Depth 3 | Set-Content $script:configPath
}

function Set-GlobalValorantPaksPath {
    if ($script:config.ValorantPaksPath -and (Test-Path $script:config.ValorantPaksPath)) {
        Add-Log "Found saved Valorant path."
        return
    }

    Add-Log "No saved path. Checking registry..." -Color $theme.LogInfo
    $regPaths = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Riot Game valorant.live", "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Riot Game valorant.live"
    foreach ($regKey in $regPaths) {
        if (Test-Path $regKey) {
            $installLocation = (Get-ItemProperty -Path $regKey).InstallLocation
            $paksPathCandidate = Join-Path $installLocation "live\ShooterGame\Content\Paks"
            if ($installLocation -and (Test-Path $paksPathCandidate)) {
                Add-Log "Found Valorant in registry. Saving for future use."
                $script:config.ValorantPaksPath = $paksPathCandidate
                Save-Configuration
                return
            }
        }
    }

    Add-Log "Could not find path automatically. Please locate it manually." -Color $theme.LogInfo
    $folderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog
    $folderBrowser.Description = "Please select your VALORANT 'live' folder (e.g., C:\Riot Games\VALORANT\live)"
    if ($folderBrowser.ShowDialog() -eq "OK") {
        $paksPathCandidate = Join-Path $folderBrowser.SelectedPath "ShooterGame\Content\Paks"
        if (Test-Path $paksPathCandidate) {
            Add-Log "Valorant path confirmed. Saving for future use."
            $script:config.ValorantPaksPath = $paksPathCandidate
            Save-Configuration
            return
        } else { throw "Invalid folder selected. The 'ShooterGame\Content\Paks' subfolder was not found. Please try again." }
    } else { throw "Operation cancelled by user." }
}

function Process-IniFile {
    param ([string]$FilePath, [string]$Width, [string]$Height)
    if (-not (Test-Path $FilePath)) { return }
    Add-Log "Processing: $($FilePath.Split('\')[-3..-1] -join '\')"
    $fileObject = Get-Item $FilePath
    $wasReadOnly = $fileObject.IsReadOnly
    try {
        if ($wasReadOnly) { $fileObject.IsReadOnly = $false }
        $newContent = Get-Content $FilePath | ForEach-Object {
            switch -Wildcard ($_){
                'ResolutionSizeX=*'                     { "ResolutionSizeX=$Width" }
                'ResolutionSizeY=*'                     { "ResolutionSizeY=$Height" }
                'LastUserConfirmedResolutionSizeX=*'    { "LastUserConfirmedResolutionSizeX=$Width" }
                'LastUserConfirmedResolutionSizeY=*'    { "LastUserConfirmedResolutionSizeY=$Height" }
                'DesiredScreenWidth=*'                  { "DesiredScreenWidth=$Width" }
                'DesiredScreenHeight=*'                 { "DesiredScreenHeight=$Height" }
                'LastUserConfirmedDesiredScreenWidth=*' { "LastUserConfirmedDesiredScreenWidth=$Width" }
                'LastUserConfirmedDesiredScreenHeight=' { "LastUserConfirmedDesiredScreenHeight=$Height" }
                'bShouldLetterbox=*'                    { "bShouldLetterbox=False" }
                'LastConfirmedShouldLetterbox=*'        { "LastConfirmedShouldLetterbox=False" }
                'bUseVSync=*'                           { "bUseVSync=False" }
                'bUseDynamicResolution=*'               { "bUseDynamicResolution=False" }
                'LastConfirmedFullscreenMode=*'         { "LastConfirmedFullscreenMode=2" }
                'PreferredFullscreenMode=*'             { "PreferredFullscreenMode=2" }
                'FullscreenMode=*'                      { "FullscreenMode=2" }
                default                                 { $_ }
            }
        }
        Set-Content -Path $FilePath -Value $newContent
    } finally {
        if ($wasReadOnly) { (Get-Item $FilePath).IsReadOnly = $true }
    }
}

# --- GUI Creation ---

function New-StyledButton {
    param ($Text, $X, $Y, $Width, $Height, $BackColor)
    $button = New-Object System.Windows.Forms.Button
    $button.Text = $Text
    $button.Location = New-Object System.Drawing.Point($X, $Y)
    $button.Size = New-Object System.Drawing.Size($Width, $Height)
    $button.BackColor = $BackColor
    $button.ForeColor = $theme.OffWhite
    $button.Font = (New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold))
    $button.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $button.FlatAppearance.BorderSize = 0
    $button.Cursor = [System.Windows.Forms.Cursors]::Hand
    $button.FlatAppearance.MouseOverBackColor = $theme.LightCharcoal
    return $button
}

function New-InputField {
    param ($Text, $X, $Y, $Width, $Value)
    $textBox = New-Object System.Windows.Forms.TextBox
    $textBox.Text = $Value
    $textBox.Location = New-Object System.Drawing.Point($X, $Y)
    $textBox.Size = New-Object System.Drawing.Size($Width, 35)
    $textBox.BackColor = $theme.DarkerCharcoal
    $textBox.ForeColor = $theme.OffWhite
    $textBox.BorderStyle = [System.Windows.Forms.BorderStyle]::FixedSingle
    $textBox.Font = (New-Object System.Drawing.Font("Segoe UI", 11))

    $label = New-Object System.Windows.Forms.Label
    $label.Text = $Text
    $label.Location = New-Object System.Drawing.Point($X, ($Y - 25))
    $label.Size = New-Object System.Drawing.Size($Width, 20)
    $label.ForeColor = $theme.OffWhite
    return @($textBox, $label)
}

function Create-MainForm {
    # Fonts
    $font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Regular)
    $titleFont = New-Object System.Drawing.Font("Segoe UI", 12, [System.Drawing.FontStyle]::Bold)

    # Form setup
    $mainForm = New-Object System.Windows.Forms.Form
    $mainForm.Text = "Valorant Essentials"
    $mainForm.Size = New-Object System.Drawing.Size(500, 550)
    $mainForm.StartPosition = "CenterScreen"
    $mainForm.FormBorderStyle = "FixedDialog"
    $mainForm.MaximizeBox = $false
    $mainForm.BackColor = $theme.DarkCharcoal
    $mainForm.Padding = New-Object System.Windows.Forms.Padding(20)
    $mainForm.Font = $font

    # Main buttons
    $updatePaksButton = New-StyledButton -Text "INSTALL / UPDATE BLOOD PAKS" -X 20 -Y 20 -Width 440 -Height 45 -BackColor $theme.ValorantRed

    # Resolution Group
    $stretchGroup = New-Object System.Windows.Forms.GroupBox
    $stretchGroup.Location = New-Object System.Drawing.Point(20, 85)
    $stretchGroup.Size = New-Object System.Drawing.Size(440, 200)
    $stretchGroup.Text = "STRETCHED RESOLUTION"
    $stretchGroup.ForeColor = $theme.OffWhite
    $stretchGroup.BackColor = $theme.DarkerCharcoal
    $stretchGroup.Font = $font

    $inputFields = New-InputField -Text "WIDTH" -X 20 -Y 60 -Width 180 -Value "1440"
    $widthTextBox = $inputFields[0]; $widthLabel = $inputFields[1]

    $inputFields = New-InputField -Text "HEIGHT" -X 220 -Y 60 -Width 180 -Value "1080"
    $heightTextBox = $inputFields[0]; $heightLabel = $inputFields[1]

    $startLauncherButton = New-StyledButton -Text "START STRETCHED RESOLUTION" -X 20 -Y 120 -Width 400 -Height 45 -BackColor $theme.ValorantRed

    # Log section
    $logLabel = New-Object System.Windows.Forms.Label
    $logLabel.Text = "STATUS LOG"
    $logLabel.Location = New-Object System.Drawing.Point(20, 305)
    $logLabel.Size = New-Object System.Drawing.Size(440, 25)
    $logLabel.ForeColor = $theme.OffWhite
    $logLabel.Font = $titleFont

    $logBox = New-Object System.Windows.Forms.RichTextBox
    $logBox.Location = New-Object System.Drawing.Point(20, 340)
    $logBox.Size = New-Object System.Drawing.Size(440, 150)
    $logBox.ReadOnly = $true
    $logBox.BackColor = $theme.DarkerCharcoal
    $logBox.ForeColor = $theme.OffWhite
    $logBox.Font = New-Object System.Drawing.Font("Consolas", 9)
    $logBox.BorderStyle = [System.Windows.Forms.BorderStyle]::FixedSingle
    $script:logBox = $logBox

    # Add controls
    $stretchGroup.Controls.AddRange(@($widthLabel, $widthTextBox, $heightLabel, $heightTextBox, $startLauncherButton))
    $mainForm.Controls.AddRange(@($updatePaksButton, $stretchGroup, $logLabel, $logBox))

    return $mainForm, $updatePaksButton, $startLauncherButton, $widthTextBox, $heightTextBox
}

# --- SCRIPT ENTRY POINT ---

# Create and unpack GUI components
$formComponents = Create-MainForm
$mainForm = $formComponents[0]
$updatePaksButton = $formComponents[1]
$startLauncherButton = $formComponents[2]
$widthTextBox = $formComponents[3]
$heightTextBox = $formComponents[4]

# --- Event Handlers & Timers ---

$monitoringTimer = New-Object System.Windows.Forms.Timer
$monitoringTimer.Interval = 5000

$monitoringTimer.Add_Tick({
    $valorantIsRunning = (Get-Process -Name "VALORANT-Win64-Shipping" -ErrorAction SilentlyContinue) -ne $null
    if ($valorantIsRunning -and -not $script:isStretched) {
        Add-Log "Valorant detected! Switching resolution..." -Color $theme.LogAction
        Start-Process -FilePath $script:qresPath -ArgumentList "/x:$($widthTextBox.Text) /y:$($heightTextBox.Text)" -NoNewWindow
        $script:isStretched = $true
    } elseif (-not $valorantIsRunning -and $script:isStretched) {
        Add-Log "Valorant closed. Reverting resolution..." -Color $theme.LogAction
        Start-Process -FilePath $script:qresPath -ArgumentList "/x:$($script:nativeResolution.Width) /y:$($script:nativeResolution.Height)" -NoNewWindow
        $script:isStretched = $false
    }
})

$updatePaksButton.Add_Click({
    $mainForm.Cursor = "WaitCursor"; $updatePaksButton.Enabled = $false; $startLauncherButton.Enabled = $false
    try {
        Add-Log "Starting 'Blood Paks' update..."
        if (-not $script:config.ValorantPaksPath) { throw "Valorant path is not set. Could not complete operation." }

        Add-Log "Deleting old VNG files..."; Remove-Item -Path (Join-Path $script:config.ValorantPaksPath "VNGLogo-WindowsClient.*") -Force -ErrorAction SilentlyContinue

        Add-Log "Downloading new MatureData files..."
        $tempDownloadPath = Join-Path $env:TEMP "ValorantPaksDownload_$(Get-Random)"
        New-Item -Path $tempDownloadPath -ItemType Directory -Force | Out-Null
        $filesToDownload = "MatureData-WindowsClient.pak", "MatureData-WindowsClient.sig", "MatureData-WindowsClient.ucas", "MatureData-WindowsClient.utoc"
        foreach ($file in $filesToDownload) {
            Invoke-WebRequest -Uri "$($script:config.PaksRepoUrl)/$file" -OutFile (Join-Path $tempDownloadPath $file)
        }

        Add-Log "Copying files to Valorant..."; Copy-Item -Path "$tempDownloadPath\*" -Destination $script:config.ValorantPaksPath -Force
        Remove-Item -Path $tempDownloadPath -Recurse -Force

        Add-Log "Update complete!" -Color $theme.LogSuccess
    } catch {
        Add-Log "[ERROR] $($_.Exception.Message)" -Color $theme.LogError
    } finally {
        $mainForm.Cursor = "Default"; $updatePaksButton.Enabled = $true; $startLauncherButton.Enabled = $true
    }
})

$startLauncherButton.Add_Click({
    $mainForm.Cursor = "WaitCursor"; $startLauncherButton.Enabled = $false
    try {
        Add-Log "Starting Stretched Res setup..."
        $stretchedWidth = $widthTextBox.Text; $stretchedHeight = $heightTextBox.Text
        $configBasePath = Join-Path $env:USERPROFILE "AppData\Local\VALORANT\Saved\Config"
        if (-not (Test-Path $configBasePath)) { throw "Valorant config directory not found." }
        $configFolders = Get-ChildItem -Path $configBasePath -Directory | Where-Object { $_.Name -ne "CrashReportClient" }
        if ($configFolders.Count -eq 0) { throw "No config folders found. Launch Valorant to main menu once." }

        Add-Log "Patching config files..."
        foreach ($folder in $configFolders) {
            $iniFilePath = Join-Path $folder.FullName "WindowsClient\GameUserSettings.ini"
            if (-not (Test-Path $iniFilePath)) {
                 $iniFilePath = Join-Path $folder.Parent.FullName "GameUserSettings.ini"
            }
            Process-IniFile -FilePath $iniFilePath -Width $stretchedWidth -Height $stretchedHeight
        }
        $monitoringTimer.Start()
        Add-Log "Setup complete! Monitoring started." -Color $theme.LogSuccess
        Add-Log "KEEP THIS APP OPEN WHILE YOU PLAY." -Color $theme.LogInfo
    } catch {
        Add-Log "[ERROR] $($_.Exception.Message)" -Color $theme.LogError
        $startLauncherButton.Enabled = $true
    } finally {
        $mainForm.Cursor = "Default"
    }
})

$mainForm.Add_FormClosing({
    $monitoringTimer.Stop()
    if ($script:isStretched) {
        Add-Log "Closing app, reverting resolution..." -Color $theme.LogAction
        Start-Process -FilePath $script:qresPath -ArgumentList "/x:$($script:nativeResolution.Width) /y:$($script:nativeResolution.Height)" -NoNewWindow
    }
})

$mainForm.Add_Load({
    try {
        Add-Log "Initializing..."
        Load-Configuration
        $script:nativeResolution = @{ Width = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Width; Height = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Height }
        Add-Log "Native resolution captured: $($script:nativeResolution.Width)x$($script:nativeResolution.Height)"

        if (-not (Test-Path $script:qresPath)) {
            Add-Log "Resolution tool not found. Downloading..." -Color $theme.LogInfo
            Invoke-WebRequest -Uri $script:config.QResUrl -OutFile $script:qresPath
            if (-not (Test-Path $script:qresPath)) { throw "Download failed." }
            Add-Log "Download complete." -Color $theme.LogSuccess
        } else {
            Add-Log "Resolution tool (QRes.exe) found."
        }

        Set-GlobalValorantPaksPath

    } catch {
        Add-Log "[ERROR] Could not initialize: $($_.Exception.Message)" -Color $theme.LogError
    }
})

[void]$mainForm.ShowDialog()
