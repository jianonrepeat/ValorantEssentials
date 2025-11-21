using System.Diagnostics;
using System.Drawing;


namespace ValorantEssentials.Services
{
    public interface IResolutionService
    {
        Size GetNativeResolution();
        Task<bool> SwitchResolutionAsync(int width, int height, string qresPath);
        Task<bool> EnsureQResAvailableAsync(string destinationPath, IProgress<int>? progress = null);
    }

    public class ResolutionService : IResolutionService
    {
        private const string QRES_URL = "https://github.com/jianonrepeat/ScreenResolutionChanger/raw/refs/heads/master/QRes.exe";
        private const int QRES_TIMEOUT_MS = 10000;
        private readonly ILogger? _logger;

        public ResolutionService(ILogger? logger = null)
        {
            _logger = logger;
        }

        public Size GetNativeResolution()
        {
            try
            {
                var resolution = Screen.PrimaryScreen?.Bounds.Size ?? new Size(1920, 1080);
                _logger?.LogDebug($"Detected native resolution: {resolution.Width}x{resolution.Height}");
                return resolution;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Failed to detect native resolution, using fallback: {ex.Message}");
                return new Size(1920, 1080); // Fallback to common resolution
            }
        }

        public async Task<bool> SwitchResolutionAsync(int width, int height, string qresPath)
        {
            if (!File.Exists(qresPath))
            {
                _logger?.LogError($"QRes.exe not found at: {qresPath}");
                return false;
            }

            try
            {
                _logger?.LogInfo($"Attempting to switch resolution to {width}x{height}");
                
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = qresPath,
                        Arguments = $"/x:{width} /y:{height}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };

                process.Start();
                _logger?.LogDebug($"QRes.exe started with PID: {process.Id}");
                
                // Wait for process to complete with timeout
                var completed = await Task.Run(() => process.WaitForExit(QRES_TIMEOUT_MS));
                
                if (!completed)
                {
                    try { process.Kill(); } catch { }
                    _logger?.LogWarning($"QRes.exe timed out after {QRES_TIMEOUT_MS}ms");
                    return false;
                }

                var success = process.ExitCode == 0;
                if (success)
                {
                    _logger?.LogSuccess($"Resolution successfully switched to {width}x{height}");
                }
                else
                {
                    _logger?.LogError($"QRes.exe failed with exit code: {process.ExitCode}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "resolution switching");
                return false;
            }
        }

        public async Task<bool> EnsureQResAvailableAsync(string destinationPath, IProgress<int>? progress = null)
        {
            if (File.Exists(destinationPath))
            {
                _logger?.LogDebug($"QRes.exe already exists at: {destinationPath}");
                return true;
            }

            try
            {
                _logger?.LogInfo($"Downloading QRes.exe to: {destinationPath}");
                var downloadProgress = progress != null ?
                    new Progress<DownloadProgress>(p => progress.Report(p.Percentage)) : null;

                var result = await FileDownloader.DownloadFileAsync(QRES_URL, destinationPath, downloadProgress);
                
                if (result.IsSuccess)
                {
                    _logger?.LogSuccess($"QRes.exe downloaded successfully ({result.BytesDownloaded} bytes)");
                    if (result.Duration.HasValue)
                    {
                        _logger?.LogPerformance("QRes download", result.Duration.Value);
                    }
                }
                else
                {
                    _logger?.LogError($"Failed to download QRes.exe: {result.ErrorMessage}");
                }
                
                return result.IsSuccess;
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "QRes download");
                return false;
            }
        }
    }
}
