# Valorant Essentials - Project Documentation

## Overview
Valorant Essentials is a C# Windows Forms application designed to enhance the Valorant gaming experience by providing tools for Blood Paks management and stretched resolution configuration.

## Architecture

### Design Pattern
The application follows a **Service-Oriented Architecture** with dependency injection for better testability and maintainability.

### Key Architectural Decisions
1. **Interface-based Services**: All major components implement interfaces for loose coupling
2. **Centralized Service Management**: ServiceManager handles dependency injection and lifecycle
3. **Event-Driven Communication**: Components communicate through events for decoupling
4. **Async/Await Pattern**: Non-blocking operations for UI responsiveness
5. **Comprehensive Logging**: Centralized logging system with multiple levels

## Project Structure

```
ValorantEssentials/
├── Models/                    # Data models and configuration
│   └── AppConfiguration.cs   # Application settings management
├── Services/                 # Business logic and service layer
│   ├── ServiceManager.cs   # Dependency injection container
│   └── ValidationService.cs # Input validation
├── Utilities/               # Helper classes and utilities
│   ├── FileDownloader.cs   # HTTP file download with progress
│   ├── IniFileHelper.cs    # INI file parsing and modification
│   ├── Logger.cs           # Comprehensive logging system
│   ├── ProcessMonitor.cs   # Valorant process monitoring
│   ├── RegistryHelper.cs   # Windows registry operations
│   └── ResolutionHelper.cs # Display resolution management
├── MainForm.cs             # Main Windows Forms UI
├── Program.cs              # Application entry point
├── ValorantEssentials.csproj # Project configuration
└── config.json             # User configuration (created at runtime)
```

## Components Documentation

### ServiceManager
- **Purpose**: Centralized dependency injection and service lifecycle management
- **Key Features**:
  - Manages service instantiation and disposal
  - Provides access to all services through properties
  - Implements IDisposable for proper cleanup

### Logger
- **Purpose**: Thread-safe logging with event notifications
- **Key Features**:
  - Multiple log levels (Debug, Info, Success, Warning, Error, Action)
  - Thread-safe StringBuilder implementation
  - Event-driven log notifications to UI
  - Extension methods for common scenarios

### FileDownloader
- **Purpose**: Optimized HTTP file downloading with progress reporting
- **Key Features**:
  - Static HttpClient for connection pooling
  - Progress reporting with IProgress<T>
  - Cancellation token support
  - Automatic cleanup of partial downloads
  - Performance timing

### ProcessMonitor
- **Purpose**: Real-time monitoring of Valorant process
- **Key Features**:
  - Timer-based process checking
  - Event-driven process start/stop notifications
  - Configurable monitoring interval
  - Thread-safe implementation

### ResolutionService
- **Purpose**: Safe display resolution switching
- **Key Features**:
  - QRes.exe integration
  - Native resolution detection
  - Automatic QRes.exe download if missing
  - Timeout protection for resolution operations

### RegistryService
- **Purpose**: Windows registry operations for Valorant detection
- **Key Features**:
  - Multi-path registry searching
  - Safe registry access with error handling
  - Valorant installation path detection
  - Paks directory validation

### IniFileService
- **Purpose**: Valorant configuration file management
- **Key Features**:
  - GameUserSettings.ini file discovery
  - Resolution setting modification
  - Read-only attribute management
  - File validation

### ValidationService
- **Purpose**: Input validation for user inputs
- **Key Features**:
  - Resolution validation with aspect ratio checking
  - Path validation
  - URL validation
  - Structured validation results

## Error Handling Strategy

### Levels of Error Handling
1. **Service Level**: Each service handles its own exceptions with logging
2. **UI Level**: MainForm handles service-level exceptions with user feedback
3. **Application Level**: Global exception handling for unhandled exceptions

### Error Handling Patterns
- **Try-Catch-Log**: All operations are wrapped in try-catch with logging
- **Graceful Degradation**: Operations continue even if non-critical parts fail
- **User Feedback**: Clear error messages for user-facing operations
- **Resource Cleanup**: Proper disposal of resources even on error

## Performance Optimizations

### Memory Management
- **StringBuilder**: Used for string concatenation in logging
- **Using Statements**: Proper disposal of file streams and HTTP clients
- **Static HttpClient**: Single instance for connection pooling
- **Event Unsubscription**: Proper cleanup of event handlers

### Threading
- **Async/Await**: Non-blocking operations for UI responsiveness
- **Task.Run**: CPU-bound operations moved to background threads
- **CancellationToken**: Proper cancellation support for long operations

### Resource Management
- **File Operations**: Buffered file operations with proper disposal
- **HTTP Requests**: Connection reuse and compression support
- **Process Monitoring**: Efficient timer-based checking

## Security Considerations

### File Operations
- **Path Validation**: All file paths are validated before use
- **Read-Only Handling**: Proper management of file attributes
- **Temporary Files**: Automatic cleanup of temporary files

### Registry Access
- **Safe Registry Reading**: Error handling for registry access
- **Path Validation**: Registry paths are validated

### Network Operations
- **HTTPS**: Secure downloads using HTTPS
- **User-Agent**: Proper user-agent headers
- **Timeout Protection**: Network operations have timeouts

## Testing Strategy

### Unit Testing
- Service interfaces enable unit testing with mocks
- Validation service has clear test cases
- File operations can be tested with temporary files

### Integration Testing
- End-to-end testing of Blood Paks installation
- Resolution switching testing
- Process monitoring testing

### Manual Testing
- UI responsiveness testing
- Error scenario testing
- Performance testing with large files

## Deployment

### Build Configuration
- **Release Mode**: Optimized builds with no debug symbols
- **Self-Contained**: Includes .NET runtime for deployment
- **Single File**: Executable includes all dependencies

### Distribution
- **GitHub Releases**: Automated releases via GitHub Actions
- **Multi-Architecture**: Both x64 and x86 builds
- **Compressed Archives**: ZIP files for easy distribution

## Future Enhancements

### Potential Features
1. **Automatic Updates**: Self-updating mechanism
2. **Multiple Game Support**: Extend to other games
3. **Profile Management**: Save/load different configurations
4. **Advanced Resolution Options**: More resolution customization
5. **Backup/Restore**: Full configuration backup and restore

### Technical Improvements
1. **Localization**: Multi-language support
2. **Theming**: Custom UI themes
3. **Logging to File**: Persistent log files
4. **Performance Metrics**: Detailed performance monitoring
5. **Plugin Architecture**: Extensible plugin system

## Maintenance Guidelines

### Code Quality
- Follow C# coding conventions
- Maintain interface-based design
- Keep services focused and single-purpose
- Document public APIs
- Write unit tests for new features

### Performance Monitoring
- Monitor memory usage
- Profile long-running operations
- Optimize file operations
- Review async/await usage

### Security Updates
- Keep dependencies updated
- Review security advisories
- Validate user inputs
- Secure file operations

This documentation should be kept updated as the project evolves.