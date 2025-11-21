using ValorantEssentials.Models;


namespace ValorantEssentials.Services
{
    public interface IServiceManager : IDisposable
    {
        ILogger Logger { get; }
        IProcessMonitor ProcessMonitor { get; }
        IResolutionService ResolutionService { get; }
        IRegistryService RegistryService { get; }
        IIniFileService IniFileService { get; }
        IBloodPaksService BloodPaksService { get; }
        AppConfiguration Configuration { get; }
        
        void InitializeServices();
        void CleanupServices();
    }

    public class ServiceManager : IServiceManager, IDisposable
    {
        private readonly string _configPath;
        private AppConfiguration? _configuration;
        private ILogger? _logger;
        private IProcessMonitor? _processMonitor;
        private IResolutionService? _resolutionService;
        private IRegistryService? _registryService;
        private IIniFileService? _iniFileService;
        private IBloodPaksService? _bloodPaksService;

        public ILogger Logger => _logger ?? throw new InvalidOperationException("Services not initialized");
        public IProcessMonitor ProcessMonitor => _processMonitor ?? throw new InvalidOperationException("Services not initialized");
        public IResolutionService ResolutionService => _resolutionService ?? throw new InvalidOperationException("Services not initialized");
        public IRegistryService RegistryService => _registryService ?? throw new InvalidOperationException("Services not initialized");
        public IIniFileService IniFileService => _iniFileService ?? throw new InvalidOperationException("Services not initialized");
        public IBloodPaksService BloodPaksService => _bloodPaksService ?? throw new InvalidOperationException("Services not initialized");
        public AppConfiguration Configuration => _configuration ?? throw new InvalidOperationException("Services not initialized");

        public ServiceManager(string configPath)
        {
            _configPath = configPath;
        }

        public void InitializeServices()
        {
            _configuration = AppConfiguration.LoadFromFile(_configPath);
            _logger = new Logger();
            _processMonitor = new ProcessMonitor(_logger);
            _resolutionService = new ResolutionService(_logger);
            _registryService = new RegistryService(_logger);
            _iniFileService = new IniFileService(_logger);
            _bloodPaksService = new BloodPaksService(_logger, _configuration);
        }

        public void CleanupServices()
        {
            _processMonitor?.Dispose();
            FileDownloader.Dispose();
        }

        public void Dispose()
        {
            CleanupServices();
            GC.SuppressFinalize(this);
        }
    }
}