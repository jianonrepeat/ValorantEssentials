using Microsoft.Win32;


namespace ValorantEssentials.Services
{
    public interface IRegistryService
    {
        string? FindValorantPaksPath();
        bool IsValorantInstalled();
        string? GetValorantInstallLocation();
    }

    public class RegistryService : IRegistryService
    {
        private static readonly string[] RegistryPaths = {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Riot Game valorant.live",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Riot Game valorant.live"
        };

        private const string INSTALL_LOCATION_VALUE = "InstallLocation";
        private const string PAKS_SUBPATH = @"live\ShooterGame\Content\Paks";
        private readonly ILogger? _logger;

        public RegistryService(ILogger? logger = null)
        {
            _logger = logger;
        }

        public string? FindValorantPaksPath()
        {
            var installLocation = GetValorantInstallLocation();
            if (string.IsNullOrEmpty(installLocation))
            {
                _logger?.LogDebug("Valorant install location not found in registry");
                return null;
            }

            var paksPath = Path.Combine(installLocation, PAKS_SUBPATH);
            var exists = Directory.Exists(paksPath);
            
            if (exists)
            {
                _logger?.LogDebug($"Valorant Paks path found: {paksPath}");
            }
            else
            {
                _logger?.LogWarning($"Valorant Paks path not found: {paksPath}");
            }
            
            return exists ? paksPath : null;
        }

        public bool IsValorantInstalled()
        {
            var paksPath = FindValorantPaksPath();
            var installed = paksPath != null;
            
            _logger?.LogInfo(installed ? "Valorant installation detected" : "Valorant installation not found");
            return installed;
        }

        public string? GetValorantInstallLocation()
        {
            foreach (var regPath in RegistryPaths)
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(regPath);
                    if (key != null)
                    {
                        var installLocation = key.GetValue(INSTALL_LOCATION_VALUE) as string;
                        if (!string.IsNullOrEmpty(installLocation))
                        {
                            _logger?.LogDebug($"Valorant install location found: {installLocation}");
                            return installLocation;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Error reading registry path {regPath}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Error reading registry path {regPath}: {ex.Message}");
                }
            }
            
            _logger?.LogDebug("Valorant install location not found in any registry path");
            return null;
        }
    }
}
