using System.Net.Http;
using System.Net;

namespace ValorantEssentials.Services
{
    public static class FileDownloader
    {
        private static readonly HttpClient _httpClient;

        static FileDownloader()
        {
            _httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ValorantEssentials/1.0");
        }

        public static async Task<DownloadResult> DownloadFileAsync(
            string url, 
            string destinationPath, 
            IProgress<DownloadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                return DownloadResult.Failure("Download URL cannot be empty");
            
            if (string.IsNullOrWhiteSpace(destinationPath))
                return DownloadResult.Failure("Destination path cannot be empty");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    return DownloadResult.Failure($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
                }

                var totalBytes = response.Content.Headers.ContentLength ?? -1;
                using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                var totalRead = 0L;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    totalRead += bytesRead;

                    if (progress != null && totalBytes > 0)
                    {
                        var percentage = (int)((totalRead * 100) / totalBytes);
                        progress.Report(new DownloadProgress(percentage, totalRead, totalBytes));
                    }
                }

                stopwatch.Stop();
                return DownloadResult.Success(totalRead, stopwatch.Elapsed);
            }
            catch (OperationCanceledException)
            {
                // Clean up partial file on cancellation
                if (File.Exists(destinationPath))
                {
                    try { File.Delete(destinationPath); } catch { }
                }
                return DownloadResult.Failure("Download cancelled");
            }
            catch (HttpRequestException ex)
            {
                // Clean up partial file on error
                if (File.Exists(destinationPath))
                {
                    try { File.Delete(destinationPath); } catch { }
                }
                return DownloadResult.Failure($"Network error: {ex.Message}");
            }
            catch (IOException ex)
            {
                // Clean up partial file on error
                if (File.Exists(destinationPath))
                {
                    try { File.Delete(destinationPath); } catch { }
                }
                return DownloadResult.Failure($"File error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Clean up partial file on error
                if (File.Exists(destinationPath))
                {
                    try { File.Delete(destinationPath); } catch { }
                }
                return DownloadResult.Failure($"Unexpected error: {ex.Message}");
            }
        }

        public static async Task<BatchDownloadResult> DownloadFilesAsync(
            Dictionary<string, string> files, 
            string destinationDirectory,
            IProgress<BatchDownloadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            var results = new List<FileDownloadResult>();
            var completed = 0;

            foreach (var kvp in files)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var fileName = Path.GetFileName(kvp.Key);
                var destinationPath = Path.Combine(destinationDirectory, fileName);
                
                var fileProgress = new Progress<DownloadProgress>(p =>
                {
                    progress?.Report(new BatchDownloadProgress(
                        fileName, 
                        p.Percentage, 
                        completed, 
                        files.Count,
                        p.BytesDownloaded,
                        p.TotalBytes));
                });

                var result = await DownloadFileAsync(kvp.Value, destinationPath, fileProgress, cancellationToken);
                results.Add(new FileDownloadResult(fileName, result));
                
                if (result.IsSuccess)
                    completed++;
            }

            return new BatchDownloadResult(results, completed, files.Count);
        }

        public static void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public record DownloadResult(bool IsSuccess, string? ErrorMessage = null, long BytesDownloaded = 0, TimeSpan? Duration = null)
    {
        public static DownloadResult Success(long bytesDownloaded, TimeSpan? duration = null) => new(true, null, bytesDownloaded, duration);
        public static DownloadResult Failure(string error) => new(false, error, 0);
    }

    public record DownloadProgress(int Percentage, long BytesDownloaded, long TotalBytes);

    public record FileDownloadResult(string FileName, DownloadResult Result);

    public record BatchDownloadResult(List<FileDownloadResult> Results, int Completed, int Total)
    {
        public bool AllSuccess => Results.All(r => r.Result.IsSuccess);
        public string Summary => $"{Completed}/{Total} files downloaded successfully";
    }

    public record BatchDownloadProgress(
        string CurrentFile, 
        int CurrentPercentage, 
        int CompletedFiles, 
        int TotalFiles,
        long CurrentBytesDownloaded,
        long CurrentTotalBytes);
}
