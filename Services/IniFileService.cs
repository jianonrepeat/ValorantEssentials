using System.Text.RegularExpressions;


namespace ValorantEssentials.Services
{
    public interface IIniFileService
    {
        void UpdateResolutionSettings(string filePath, int width, int height);
        List<string> FindGameUserSettingsFiles();
        bool ValidateSettingsFile(string filePath);
        bool ResetAllConfigurations();
        void ApplyResolutionToAllConfigs(int width, int height);
    }

    public class IniFileService : IIniFileService
    {
        private const string VALORANT_CONFIG_PATH = @"VALORANT\Saved\Config";
        private const string GAME_USER_SETTINGS_FILENAME = "GameUserSettings.ini";
        private const string CRASH_REPORT_CLIENT_FOLDER = "CrashReportClient";
        private readonly ILogger? _logger;

        private static readonly Dictionary<string, string> ResolutionSettings = new()
        {
            ["ResolutionSizeX"] = "",
            ["ResolutionSizeY"] = "",
            ["LastUserConfirmedResolutionSizeX"] = "",
            ["LastUserConfirmedResolutionSizeY"] = "",
            ["DesiredScreenWidth"] = "",
            ["DesiredScreenHeight"] = "",
            ["LastUserConfirmedDesiredScreenWidth"] = "",
            ["LastUserConfirmedDesiredScreenHeight"] = "",
            ["bShouldLetterbox"] = "False",
            ["LastConfirmedShouldLetterbox"] = "False",
            ["bUseVSync"] = "False",
            ["bUseDynamicResolution"] = "False",
            ["LastConfirmedFullscreenMode"] = "2",
            ["PreferredFullscreenMode"] = "2",
            ["FullscreenMode"] = "2"
        };

        public IniFileService(ILogger? logger = null)
        {
            _logger = logger;
        }

        public void UpdateResolutionSettings(string filePath, int width, int height)
        {
            if (!File.Exists(filePath))
            {
                _logger?.LogWarning($"Settings file not found: {filePath}");
                return;
            }

            var fileInfo = new FileInfo(filePath);
            var wasReadOnly = fileInfo.IsReadOnly;

            try
            {
                if (wasReadOnly)
                {
                    _logger?.LogDebug($"Removing read-only attribute from: {filePath}");
                    fileInfo.IsReadOnly = false;
                }

                var settingsToUpdate = new Dictionary<string, string>(ResolutionSettings);
                settingsToUpdate["ResolutionSizeX"] = width.ToString();
                settingsToUpdate["ResolutionSizeY"] = height.ToString();
                settingsToUpdate["LastUserConfirmedResolutionSizeX"] = width.ToString();
                settingsToUpdate["LastUserConfirmedResolutionSizeY"] = height.ToString();
                settingsToUpdate["DesiredScreenWidth"] = width.ToString();
                settingsToUpdate["DesiredScreenHeight"] = height.ToString();
                settingsToUpdate["LastUserConfirmedDesiredScreenWidth"] = width.ToString();
                settingsToUpdate["LastUserConfirmedDesiredScreenHeight"] = height.ToString();

                var lines = File.ReadAllLines(filePath);
                var modified = false;
                var modifiedSettings = new List<string>();

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    foreach (var setting in settingsToUpdate)
                    {
                        var pattern = $"^{Regex.Escape(setting.Key)}=.*$";
                        if (Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase))
                        {
                            var oldValue = line.Split('=')[1];
                            var newValue = setting.Value;
                            if (oldValue != newValue)
                            {
                                lines[i] = $"{setting.Key}={newValue}";
                                modified = true;
                                modifiedSettings.Add($"{setting.Key}: {oldValue} -> {newValue}");
                            }
                            break;
                        }
                    }
                }

                if (modified)
                {
                    File.WriteAllLines(filePath, lines);
                    _logger?.LogSuccess($"Updated resolution settings in {filePath}: {string.Join(", ", modifiedSettings)}");
                }
                else
                {
                    _logger?.LogDebug($"No changes needed in {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "resolution settings update");
                throw;
            }
            finally
            {
                if (wasReadOnly)
                {
                    _logger?.LogDebug($"Restoring read-only attribute to: {filePath}");
                    fileInfo.IsReadOnly = true;
                }
            }
        }

        public List<string> FindGameUserSettingsFiles()
        {
            var configBasePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                VALORANT_CONFIG_PATH);

            if (!Directory.Exists(configBasePath))
            {
                _logger?.LogWarning($"Valorant config directory not found: {configBasePath}");
                return new List<string>();
            }

            try
            {
                var files = Directory.GetFiles(configBasePath, GAME_USER_SETTINGS_FILENAME, SearchOption.AllDirectories)
                    .Where(f => !f.Contains(CRASH_REPORT_CLIENT_FOLDER, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                _logger?.LogInfo($"Found {files.Count} GameUserSettings.ini files");
                foreach (var file in files)
                {
                    _logger?.LogDebug($"Found settings file: {file}");
                }
                
                return files;
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "finding GameUserSettings files");
                return new List<string>();
            }
        }

        public bool ValidateSettingsFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _logger?.LogWarning($"Settings file not found for validation: {filePath}");
                return false;
            }

            try
            {
                var content = File.ReadAllText(filePath);
                var hasResolutionX = content.Contains("ResolutionSizeX", StringComparison.OrdinalIgnoreCase);
                var hasResolutionY = content.Contains("ResolutionSizeY", StringComparison.OrdinalIgnoreCase);
                var isValid = hasResolutionX && hasResolutionY;
                
                _logger?.LogDebug($"Settings file validation for {filePath}: {(isValid ? "Valid" : "Invalid")}");
                return isValid;
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "validating settings file");
                return false;
            }
        }

        public bool ResetAllConfigurations()
        {
            try
            {
                var configBasePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    VALORANT_CONFIG_PATH);

                if (!Directory.Exists(configBasePath))
                {
                    _logger?.LogWarning("Valorant config directory not found. No configurations to reset.");
                    return false;
                }

                var files = Directory.GetFiles(configBasePath, GAME_USER_SETTINGS_FILENAME, SearchOption.AllDirectories)
                    .Where(f => !f.Contains(CRASH_REPORT_CLIENT_FOLDER, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (files.Count == 0)
                {
                    _logger?.LogInfo("No GameUserSettings.ini files found to reset.");
                    return true;
                }

                bool anyDeleted = false;
                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                        _logger?.LogInfo($"Deleted configuration file: {file}");
                        anyDeleted = true;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Error deleting configuration file {file}: {ex.Message}");
                    }
                }

                if (anyDeleted)
                {
                    _logger?.LogSuccess("Successfully reset all VALORANT configurations. The game will create new default config files on next launch.");
                }
                return anyDeleted;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error resetting configurations: {ex.Message}");
                return false;
            }
        }

        public void ApplyResolutionToAllConfigs(int width, int height)
        {
            var configFiles = FindGameUserSettingsFiles();
            if (configFiles.Count == 0)
            {
                throw new Exception("No Valorant config files found. Launch Valorant to main menu once.");
            }

            _logger?.LogInfo($"Found {configFiles.Count} config files. Patching...");
            
            foreach (var file in configFiles)
            {
                UpdateResolutionSettings(file, width, height);
            }
        }
    }
}
