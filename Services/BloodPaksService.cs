using ValorantEssentials.Models;


namespace ValorantEssentials.Services
{
    public interface IBloodPaksService
    {
        Task<bool> UpdatePaksAsync(IProgress<BatchDownloadProgress> progress, CancellationToken cancellationToken);
    }

    public class BloodPaksService : IBloodPaksService
    {
        private readonly ILogger _logger;
        private readonly AppConfiguration _configuration;

        public BloodPaksService(ILogger logger, AppConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> UpdatePaksAsync(IProgress<BatchDownloadProgress> progress, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_configuration.ValorantPaksPath))
            {
                _logger.LogError("Valorant path is not configured.");
                return false;
            }

            try
            {
                _logger.LogInfo("Starting Blood Paks update...");
                
                // Delete old files
                await Task.Run(() => DeleteVngFiles(), cancellationToken);
                
                // Download new files
                var files = new Dictionary<string, string>
                {
                    ["MatureData-WindowsClient.pak"] = $"{_configuration.PaksRepoUrl}/MatureData-WindowsClient.pak",
                    ["MatureData-WindowsClient.sig"] = $"{_configuration.PaksRepoUrl}/MatureData-WindowsClient.sig",
                    ["MatureData-WindowsClient.ucas"] = $"{_configuration.PaksRepoUrl}/MatureData-WindowsClient.ucas",
                    ["MatureData-WindowsClient.utoc"] = $"{_configuration.PaksRepoUrl}/MatureData-WindowsClient.utoc"
                };

                var tempPath = Path.Combine(Path.GetTempPath(), $"ValorantPaks_{Guid.NewGuid()}");

                var result = await FileDownloader.DownloadFilesAsync(files, tempPath, progress, cancellationToken);
                
                if (result.AllSuccess)
                {
                    _logger.LogInfo("Copying files to Valorant directory...");
                    await Task.Run(() => CopyFilesToValorant(tempPath), cancellationToken);
                    _logger.LogSuccess("Blood Paks updated successfully!");
                    return true;
                }
                else
                {
                    _logger.LogError($"Failed to download some files: {result.Summary}");
                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Blood Paks update cancelled");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "Blood Paks update");
                return false;
            }
        }

        private void DeleteVngFiles()
        {
            if (string.IsNullOrEmpty(_configuration.ValorantPaksPath))
                return;

            try
            {
                var vngFiles = Directory.GetFiles(_configuration.ValorantPaksPath, "VNGLogo-WindowsClient.*");
                if (vngFiles.Length == 0)
                {
                    _logger.LogDebug("No VNG files found to delete");
                    return;
                }

                foreach (var file in vngFiles)
                {
                    try
                    {
                        File.Delete(file);
                        _logger.LogInfo($"Deleted {Path.GetFileName(file)}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Could not delete {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error searching for VNG files: {ex.Message}");
            }
        }

        private void CopyFilesToValorant(string sourcePath)
        {
            try
            {
                var files = Directory.GetFiles(sourcePath);
                if (files.Length == 0)
                {
                    _logger.LogWarning("No files found to copy");
                    return;
                }

                foreach (var file in files)
                {
                    try
                    {
                        var destFile = Path.Combine(_configuration.ValorantPaksPath, Path.GetFileName(file));
                        File.Copy(file, destFile, true);
                        _logger.LogInfo($"Copied {Path.GetFileName(file)}");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to copy {Path.GetFileName(file)}: {ex.Message}");
                    }
                }

                // Clean up temp directory
                try
                {
                    Directory.Delete(sourcePath, true);
                    _logger.LogDebug($"Cleaned up temporary directory: {sourcePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Could not clean up temp directory: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error copying files: {ex.Message}");
                throw;
            }
        }
    }
}
