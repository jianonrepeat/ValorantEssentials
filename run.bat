@echo off
:: =================================================================
:: VALORANT ESSENTIALS - STARTUP SCRIPT
:: Description: This batch file runs the main PowerShell script.
:: The PowerShell script will handle its own Administrator elevation.
:: The -WindowStyle Hidden flag is used to prevent the console from showing.
:: =================================================================

:: Set the title of the command window
TITLE ValorantEssentials Launcher

:: ---------------------------------------------------------------
:: Run the PowerShell Script
:: ---------------------------------------------------------------
echo Starting the ValorantEssentials Launcher...

:: -WindowStyle Hidden: Prevents the PowerShell console from appearing.
:: -ExecutionPolicy Bypass: Temporarily allows the script to run.
:: -File: Specifies the PowerShell script to execute.
:: "%~dp0ValorantEssentials.ps1": Finds the .ps1 file located in the same directory as this .bat file.
powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File "%~dp0ValorantEssentials.ps1"
