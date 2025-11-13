using System.Diagnostics;

namespace ValorantEssentials.Utilities
{
    public interface IProcessMonitor : IDisposable
    {
        bool IsMonitoring { get; }
        event EventHandler<ProcessEventArgs>? ProcessStarted;
        event EventHandler<ProcessEventArgs>? ProcessStopped;
        
        void StartMonitoring(int intervalMs = 5000);
        void StopMonitoring();
    }

    public class ProcessMonitor : IProcessMonitor
    {
        private const string VALORANT_PROCESS_NAME = "VALORANT-Win64-Shipping";
        private readonly System.Threading.Timer? _timer;
        private bool _isValorantRunning = false;
        private bool _isMonitoring = false;
        private ILogger? _logger;

        public event EventHandler<ProcessEventArgs>? ProcessStarted;
        public event EventHandler<ProcessEventArgs>? ProcessStopped;

        public bool IsMonitoring => _isMonitoring;

        public ProcessMonitor(ILogger? logger = null)
        {
            _logger = logger;
            _timer = new System.Threading.Timer(CheckProcess, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void StartMonitoring(int intervalMs = 5000)
        {
            if (_isMonitoring)
                return;

            _isMonitoring = true;
            _timer?.Change(0, intervalMs);
            _logger?.LogDebug($"Process monitoring started with {intervalMs}ms interval");
        }

        public void StopMonitoring()
        {
            if (!_isMonitoring)
                return;

            _isMonitoring = false;
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _logger?.LogDebug("Process monitoring stopped");
        }

        private void CheckProcess(object? state)
        {
            try
            {
                var isRunning = IsProcessRunning(VALORANT_PROCESS_NAME);

                if (isRunning && !_isValorantRunning)
                {
                    _isValorantRunning = true;
                    _logger?.LogAction($"Valorant process detected as running");
                    ProcessStarted?.Invoke(this, new ProcessEventArgs(VALORANT_PROCESS_NAME, true));
                }
                else if (!isRunning && _isValorantRunning)
                {
                    _isValorantRunning = false;
                    _logger?.LogAction($"Valorant process detected as stopped");
                    ProcessStopped?.Invoke(this, new ProcessEventArgs(VALORANT_PROCESS_NAME, false));
                }
            }
            catch (Exception ex)
            {
                // Log error but don't stop monitoring
                _logger?.LogError($"Error in process monitoring: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error in process monitoring: {ex.Message}");
            }
        }

        public static bool IsProcessRunning(string processName)
        {
            try
            {
                return Process.GetProcessesByName(processName).Length > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking process {processName}: {ex.Message}");
                return false;
            }
        }

        public static bool IsValorantRunning()
        {
            return IsProcessRunning(VALORANT_PROCESS_NAME);
        }

        public void Dispose()
        {
            StopMonitoring();
            _timer?.Dispose();
            _logger?.LogDebug("Process monitor disposed");
        }
    }

    public class ProcessEventArgs : EventArgs
    {
        public string ProcessName { get; }
        public bool IsRunning { get; }

        public ProcessEventArgs(string processName, bool isRunning)
        {
            ProcessName = processName;
            IsRunning = isRunning;
        }
    }
}