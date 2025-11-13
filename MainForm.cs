using ValorantEssentials.Models;
using ValorantEssentials.Utilities;
using ValorantEssentials.Services;
using System.Drawing;

namespace ValorantEssentials
{
    public partial class MainForm : Form
    {
        private IServiceManager _serviceManager = null!;
        private CancellationTokenSource? _cancellationTokenSource;

        // UI Controls
        private Button _updatePaksButton = null!;
        private Button _startLauncherButton = null!;
        private TextBox _widthTextBox = null!;
        private TextBox _heightTextBox = null!;
        private RichTextBox _logBox = null!;
        private ProgressBar? _progressBar;
        private Label? _statusLabel;

        // Color theme
        private readonly Color _valorantRed = Color.FromArgb(253, 69, 86);
        private readonly Color _offWhite = Color.FromArgb(250, 250, 250);
        private readonly Color _darkCharcoal = Color.FromArgb(28, 28, 28);
        private readonly Color _darkerCharcoal = Color.FromArgb(18, 18, 18);
        private readonly Color _lightCharcoal = Color.FromArgb(58, 58, 58);
        private readonly Color _logSuccess = Color.FromArgb(50, 205, 50);
        private readonly Color _logError = Color.FromArgb(255, 69, 58);
        private readonly Color _logInfo = Color.FromArgb(255, 255, 102);
        private readonly Color _logAction = Color.FromArgb(200, 160, 255);

        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
            SubscribeToEvents();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Form setup
            Text = "Valorant Essentials";
            Size = new Size(500, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = _darkCharcoal;
            Padding = new Padding(20);
            Font = new Font("Segoe UI", 10);

            // Update Paks Button
            _updatePaksButton = CreateStyledButton(
                "INSTALL / UPDATE BLOOD PAKS",
                new Point(20, 20),
                new Size(440, 45),
                _valorantRed);

            // Stretched Resolution Group
            var stretchGroup = new GroupBox
            {
                Location = new Point(20, 85),
                Size = new Size(440, 200),
                Text = "STRETCHED RESOLUTION",
                ForeColor = _offWhite,
                BackColor = _darkerCharcoal,
                Font = new Font("Segoe UI", 10)
            };

            // Width input
            var widthLabel = new Label
            {
                Text = "WIDTH",
                Location = new Point(20, 35),
                Size = new Size(180, 20),
                ForeColor = _offWhite
            };

            _widthTextBox = new TextBox
            {
                Text = "1728",
                Location = new Point(20, 60),
                Size = new Size(180, 35),
                BackColor = _darkerCharcoal,
                ForeColor = _offWhite,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11)
            };

            // Height input
            var heightLabel = new Label
            {
                Text = "HEIGHT",
                Location = new Point(220, 35),
                Size = new Size(180, 20),
                ForeColor = _offWhite
            };

            _heightTextBox = new TextBox
            {
                Text = "1080",
                Location = new Point(220, 60),
                Size = new Size(180, 35),
                BackColor = _darkerCharcoal,
                ForeColor = _offWhite,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11)
            };

            // Start Launcher Button
            _startLauncherButton = CreateStyledButton(
                "START STRETCHED RESOLUTION",
                new Point(20, 120),
                new Size(400, 45),
                _valorantRed);

            stretchGroup.Controls.AddRange(new Control[] { widthLabel, _widthTextBox, heightLabel, _heightTextBox, _startLauncherButton });

            // Progress section
            var progressPanel = new Panel
            {
                Location = new Point(20, 295),
                Size = new Size(440, 40),
                BackColor = _darkerCharcoal,
                BorderStyle = BorderStyle.FixedSingle
            };

            _progressBar = new ProgressBar
            {
                Location = new Point(10, 10),
                Size = new Size(300, 20),
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };

            _statusLabel = new Label
            {
                Location = new Point(320, 10),
                Size = new Size(110, 20),
                ForeColor = _offWhite,
                TextAlign = ContentAlignment.MiddleRight,
                Text = "Ready"
            };

            progressPanel.Controls.AddRange(new Control[] { _progressBar, _statusLabel });

            // Log Label
            var logLabel = new Label
            {
                Text = "STATUS LOG",
                Location = new Point(20, 345),
                Size = new Size(440, 25),
                ForeColor = _offWhite,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };

            // Log Box
            _logBox = new RichTextBox
            {
                Location = new Point(20, 380),
                Size = new Size(440, 150),
                ReadOnly = true,
                BackColor = _darkerCharcoal,
                ForeColor = _offWhite,
                Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add controls to form
            Controls.AddRange(new Control[] { _updatePaksButton, stretchGroup, progressPanel, logLabel, _logBox });

            // Event handlers
            _updatePaksButton.Click += UpdatePaksButton_Click;
            _startLauncherButton.Click += StartLauncherButton_Click;
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;

            ResumeLayout(false);
            PerformLayout();
        }

        private Button CreateStyledButton(string text, Point location, Size size, Color backColor)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = backColor,
                ForeColor = _offWhite,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = _lightCharcoal;

            return button;
        }

        private void InitializeServices()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            _serviceManager = new ServiceManager(configPath);
            _serviceManager.InitializeServices();
        }

        private void SubscribeToEvents()
        {
            _serviceManager.Logger.LogAdded += Logger_LogAdded;
            _serviceManager.ProcessMonitor.ProcessStarted += ProcessMonitor_ProcessStarted;
            _serviceManager.ProcessMonitor.ProcessStopped += ProcessMonitor_ProcessStopped;
        }

        private async void MainForm_Load(object? sender, EventArgs e)
        {
            LogInfo("Initializing Valorant Essentials...");
            
            var nativeResolution = _serviceManager.ResolutionService.GetNativeResolution();
            LogInfo($"Native resolution captured: {nativeResolution.Width}x{nativeResolution.Height}");

            await InitializeQResAsync();
            InitializeValorantPath();
            
            LogSuccess("Initialization complete!");
        }

        private async Task InitializeQResAsync()
        {
            var qresPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QRes.exe");
            
            if (!File.Exists(qresPath))
            {
                LogInfo("Resolution tool not found. Downloading...");
                var progress = new Progress<int>(p => UpdateProgress(p, "Downloading QRes..."));
                
                var success = await _serviceManager.ResolutionService.EnsureQResAvailableAsync(qresPath, progress);
                
                if (success)
                {
                    LogSuccess("QRes.exe downloaded successfully");
                }
                else
                {
                    LogError("Failed to download QRes.exe");
                }
            }
            else
            {
                LogInfo("QRes.exe found");
            }
        }

        private void InitializeValorantPath()
        {
            if (!string.IsNullOrEmpty(_serviceManager.Configuration.ValorantPaksPath) && 
                Directory.Exists(_serviceManager.Configuration.ValorantPaksPath))
            {
                LogInfo("Found saved Valorant path");
                return;
            }

            LogInfo("Searching for Valorant installation...");
            var paksPath = _serviceManager.RegistryService.FindValorantPaksPath();
            
            if (paksPath != null)
            {
                LogInfo("Found Valorant in registry. Saving path...");
                _serviceManager.Configuration.ValorantPaksPath = paksPath;
                _serviceManager.Configuration.SaveToFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"));
                LogSuccess("Valorant path saved");
            }
            else
            {
                LogWarning("Could not find Valorant automatically. Please select manually if needed.");
            }
        }

        private async void UpdatePaksButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_serviceManager.Configuration.ValorantPaksPath))
            {
                LogError("Valorant path is not configured. Please run the application again or select path manually.");
                return;
            }

            SetBusyState(true);
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                LogInfo("Starting Blood Paks update...");
                
                // Delete old files
                await Task.Run(() => DeleteVngFiles(), _cancellationTokenSource.Token);
                
                // Download new files
                var files = new Dictionary<string, string>
                {
                    ["MatureData-WindowsClient.pak"] = $"{_serviceManager.Configuration.PaksRepoUrl}/MatureData-WindowsClient.pak",
                    ["MatureData-WindowsClient.sig"] = $"{_serviceManager.Configuration.PaksRepoUrl}/MatureData-WindowsClient.sig",
                    ["MatureData-WindowsClient.ucas"] = $"{_serviceManager.Configuration.PaksRepoUrl}/MatureData-WindowsClient.ucas",
                    ["MatureData-WindowsClient.utoc"] = $"{_serviceManager.Configuration.PaksRepoUrl}/MatureData-WindowsClient.utoc"
                };

                var tempPath = Path.Combine(Path.GetTempPath(), $"ValorantPaks_{Guid.NewGuid()}");
                var progress = new Progress<BatchDownloadProgress>(p => 
                    UpdateProgress((p.CompletedFiles * 100 + p.CurrentPercentage) / p.TotalFiles, 
                        $"Downloading {p.CurrentFile}..."));

                var result = await FileDownloader.DownloadFilesAsync(files, tempPath, progress, _cancellationTokenSource.Token);
                
                if (result.AllSuccess)
                {
                    LogInfo("Copying files to Valorant directory...");
                    await Task.Run(() => CopyFilesToValorant(tempPath), _cancellationTokenSource.Token);
                    LogSuccess("Blood Paks updated successfully!");
                }
                else
                {
                    LogError($"Failed to download some files: {result.Summary}");
                }
            }
            catch (OperationCanceledException)
            {
                LogWarning("Blood Paks update cancelled");
            }
            catch (Exception ex)
            {
                _serviceManager.Logger.LogException(ex, "Blood Paks update");
            }
            finally
            {
                SetBusyState(false);
                HideProgress();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async void StartLauncherButton_Click(object? sender, EventArgs e)
        {
            // Validate inputs
            var validation = new ValidationService().ValidateResolution(_widthTextBox.Text, _heightTextBox.Text);
            if (!validation.IsValid)
            {
                LogError(validation.ErrorMessage!);
                return;
            }

            SetBusyState(true, false);
            
            try
            {
                LogInfo("Starting stretched resolution setup...");
                
                var width = int.Parse(_widthTextBox.Text);
                var height = int.Parse(_heightTextBox.Text);
                
                var configFiles = _serviceManager.IniFileService.FindGameUserSettingsFiles();
                if (configFiles.Count == 0)
                {
                    LogError("No Valorant config files found. Launch Valorant to main menu once.");
                    return;
                }

                LogInfo($"Found {configFiles.Count} config files. Patching...");
                
                // Use Task.Run to make file operations async
                await Task.Run(() =>
                {
                    foreach (var file in configFiles)
                    {
                        _serviceManager.IniFileService.UpdateResolutionSettings(file, width, height);
                    }
                });

                _serviceManager.ProcessMonitor.StartMonitoring();
                LogSuccess("Setup complete! Process monitoring started.");
                LogWarning("Keep this application open while playing Valorant.");
            }
            catch (Exception ex)
            {
                _serviceManager.Logger.LogException(ex, "stretched resolution setup");
            }
            finally
            {
                SetBusyState(false, true);
            }
        }

        private void ProcessMonitor_ProcessStarted(object? sender, ProcessEventArgs e)
        {
            BeginInvoke(() =>
            {
                LogAction("Valorant detected! Switching resolution...");
                
                if (int.TryParse(_widthTextBox.Text, out var width) && int.TryParse(_heightTextBox.Text, out var height))
                {
                    var qresPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QRes.exe");
                    Task.Run(async () =>
                    {
                        var success = await _serviceManager.ResolutionService.SwitchResolutionAsync(width, height, qresPath);
                        if (success)
                        {
                            LogSuccess("Resolution switched successfully");
                        }
                        else
                        {
                            LogError("Failed to switch resolution");
                        }
                    });
                }
            });
        }

        private void ProcessMonitor_ProcessStopped(object? sender, ProcessEventArgs e)
        {
            BeginInvoke(() =>
            {
                LogAction("Valorant closed. Reverting resolution...");
                
                var nativeResolution = _serviceManager.ResolutionService.GetNativeResolution();
                var qresPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QRes.exe");
                
                Task.Run(async () =>
                {
                    var success = await _serviceManager.ResolutionService.SwitchResolutionAsync(
                        nativeResolution.Width, nativeResolution.Height, qresPath);
                    
                    if (success)
                    {
                        LogSuccess("Resolution reverted successfully");
                    }
                    else
                    {
                        LogError("Failed to revert resolution");
                    }
                });
            });
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _serviceManager.ProcessMonitor.StopMonitoring();
            
            var nativeResolution = _serviceManager.ResolutionService.GetNativeResolution();
            var qresPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QRes.exe");
            
            if (File.Exists(qresPath))
            {
                LogAction("Restoring native resolution before exit...");
                try
                {
                    Task.Run(async () =>
                    {
                        await _serviceManager.ResolutionService.SwitchResolutionAsync(
                            nativeResolution.Width, nativeResolution.Height, qresPath);
                    }).Wait(TimeSpan.FromSeconds(5));
                }
                catch (Exception ex)
                {
                    LogWarning($"Failed to restore resolution on exit: {ex.Message}");
                }
            }
            
            _serviceManager.CleanupServices();
        }

        private void Logger_LogAdded(object? sender, LogEventArgs e)
        {
            BeginInvoke(() =>
            {
                var color = e.Level switch
                {
                    LogLevel.Success => _logSuccess,
                    LogLevel.Error => _logError,
                    LogLevel.Warning => _logInfo,
                    LogLevel.Action => _logAction,
                    _ => _offWhite
                };

                _logBox.SelectionColor = color;
                _logBox.AppendText($"{e.Timestamp} - {e.Message}\n");
                _logBox.ScrollToCaret();
            });
        }

        private void DeleteVngFiles()
        {
            if (string.IsNullOrEmpty(_serviceManager.Configuration.ValorantPaksPath))
                return;

            try
            {
                var vngFiles = Directory.GetFiles(_serviceManager.Configuration.ValorantPaksPath, "VNGLogo-WindowsClient.*");
                if (vngFiles.Length == 0)
                {
                    LogDebug("No VNG files found to delete");
                    return;
                }

                foreach (var file in vngFiles)
                {
                    try
                    {
                        File.Delete(file);
                        LogInfo($"Deleted {Path.GetFileName(file)}");
                    }
                    catch (Exception ex)
                    {
                        LogWarning($"Could not delete {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Error searching for VNG files: {ex.Message}");
            }
        }

        private void CopyFilesToValorant(string sourcePath)
        {
            try
            {
                var files = Directory.GetFiles(sourcePath);
                if (files.Length == 0)
                {
                    LogWarning("No files found to copy");
                    return;
                }

                foreach (var file in files)
                {
                    try
                    {
                        var destFile = Path.Combine(_serviceManager.Configuration.ValorantPaksPath, Path.GetFileName(file));
                        File.Copy(file, destFile, true);
                        LogInfo($"Copied {Path.GetFileName(file)}");
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
                    LogDebug($"Cleaned up temporary directory: {sourcePath}");
                }
                catch (Exception ex)
                {
                    LogWarning($"Could not clean up temp directory: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error copying files: {ex.Message}");
                throw;
            }
        }

        private void SetBusyState(bool busy, bool enableLauncherButton = true)
        {
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            _updatePaksButton.Enabled = !busy;
            if (enableLauncherButton)
                _startLauncherButton.Enabled = !busy;
        }

        private void UpdateProgress(int percentage, string status)
        {
            if (_progressBar != null && _statusLabel != null)
            {
                _progressBar.Value = Math.Max(0, Math.Min(100, percentage));
                _progressBar.Visible = true;
                _statusLabel.Text = status;
            }
        }

        private void HideProgress()
        {
            if (_progressBar != null && _statusLabel != null)
            {
                _progressBar.Visible = false;
                _statusLabel.Text = "Ready";
            }
        }

        // Logging helpers
        private void LogDebug(string message) => _serviceManager.Logger.LogDebug(message);
        private void LogInfo(string message) => _serviceManager.Logger.LogInfo(message);
        private void LogSuccess(string message) => _serviceManager.Logger.LogSuccess(message);
        private void LogWarning(string message) => _serviceManager.Logger.LogWarning(message);
        private void LogError(string message) => _serviceManager.Logger.LogError(message);
        private void LogAction(string message) => _serviceManager.Logger.LogAction(message);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationTokenSource?.Dispose();
                _serviceManager?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}