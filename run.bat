@echo off
:: =================================================================
:: VALORANT ESSENTIALS - STARTUP SCRIPT
:: Description: This batch file automatically runs the main PowerShell
:: script with the necessary Administrator rights and execution policy.
:: =================================================================

:: Set the title of the command window
TITLE ValorantEssentials Launcher

:: ---------------------------------------------------------------
:: 1. Check for Administrator Privileges
:: ---------------------------------------------------------------
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Requesting administrative permissions...
    :: Re-launch this batch file as an administrator
    powershell.exe -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

:: ---------------------------------------------------------------
:: 2. Run the PowerShell Script
:: ---------------------------------------------------------------
echo Starting the ValorantEssentials Launcher...

:: -ExecutionPolicy Bypass: Temporarily allows the script to run.
:: -File: Specifies the PowerShell script to execute.
:: "%~dp0ValorantEssentials.ps1": Finds the .ps1 file located in the same directory as this .bat file.
powershell.exe -ExecutionPolicy Bypass -File "%~dp0ValorantEssentials.ps1"