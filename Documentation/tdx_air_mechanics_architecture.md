# TDX Air Mechanics - System Architecture & Technical Design

## 1. High-Level Architecture
```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   MSFS 2020/    │    │  TDX Air         │    │  Force-Feedback │
│      2024       │◄──►│  Mechanics       │◄──►│    Joystick     │
│                 │    │                  │    │                 │
│  SimConnect SDK │    │  • Data Proc.    │    │  DirectInput FF │
└─────────────────┘    │  • Force Calc.   │    └─────────────────┘
                       │  • UI/Config     │
                       └──────────────────┘
```

## 2. Component Design

### 2.1 SimConnect Manager
- **Purpose**: Handle all SimConnect communications
- **Responsibilities**:
  - Establish connection to MSFS
  - Subscribe to required data variables
  - Handle connection errors and reconnection
  - Data rate management and filtering

### 2.2 Force Calculation Engine
- **Purpose**: Convert flight data to force vectors
- **Responsibilities**:
  - Apply aerodynamic models
  - Calculate control surface forces
  - Process environmental effects
  - Scale forces for hardware capabilities

### 2.3 DirectInput Manager
- **Purpose**: Interface with force-feedback hardware
- **Responsibilities**:
  - Enumerate and initialize FF devices
  - Send force-feedback effects
  - Handle device disconnection/reconnection
  - Manage effect priorities and timing

### 2.4 Configuration System
- **Purpose**: Manage user settings and aircraft profiles
- **Responsibilities**:
  - Load/save configuration files
  - Aircraft-specific force profiles
  - User preference management
  - Profile import/export

### 2.5 User Interface
- **Purpose**: Provide user control and system monitoring
- **Responsibilities**:
  - Real-time force visualization
  - Configuration interface
  - System status display
  - Calibration tools

## 3. Data Flow Architecture
```
SimConnect Data → Data Validation → Force Calculation → 
Effect Generation → DirectInput Commands → Hardware
```

## 4. Development Environment
- **Language**: C# (.NET 6.0 or later)
- **Framework**: .NET Framework/Core, WPF (UI), P/Invoke for native APIs
- **IDE**: Visual Studio 2022
- **Build System**: MSBuild/.NET SDK
- **Package Manager**: NuGet
- **Version Control**: Git

## 5. Key Libraries and Dependencies
- **SimConnect SDK**: MSFS data interface (via managed wrapper or P/Invoke)
- **SharpDX**: Managed DirectX wrapper for DirectInput
- **Newtonsoft.Json**: Configuration file handling
- **System.Numerics**: Vector/matrix calculations
- **Microsoft.Extensions.Logging**: Logging framework
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.DependencyInjection**: IoC container
- **NLog**: Advanced logging capabilities

## 6. Threading Model and Patterns
- **Main Thread (UI Thread)**: WPF UI and user interaction
- **Background Services**: Hosted services for long-running operations
- **Task-based Operations**: Async/await for I/O operations
- **Timer-based Updates**: System.Threading.Timer for periodic force updates
- **Thread-safe Collections**: ConcurrentQueue, ConcurrentDictionary for data sharing
- **Synchronization**: SemaphoreSlim, AsyncLock for resource coordination

### 6.1 Service Architecture Pattern
```csharp
public interface ISimConnectService : IHostedService
{
    event EventHandler<FlightDataEventArgs> FlightDataReceived;
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}

public interface IForceEffectService : IHostedService
{
    Task ApplyForceAsync(ForceVector force, CancellationToken cancellationToken);
    Task InitializeHardwareAsync();
}
```

## 7. Project Structure
```
TDXAirMechanics.sln
├── TDXAirMechanics.Core/              # Core business logic
│   ├── Models/                        # Data models and DTOs
│   ├── Services/                      # Business services
│   ├── Interfaces/                    # Service contracts
│   └── Extensions/                    # Extension methods
├── TDXAirMechanics.SimConnect/        # SimConnect integration
│   ├── SimConnectManager.cs
│   ├── FlightDataModels.cs
│   └── SimConnectInterop.cs           # P/Invoke declarations
├── TDXAirMechanics.DirectInput/       # Force-feedback hardware
│   ├── DirectInputManager.cs
│   ├── ForceEffectGenerator.cs
│   └── JoystickDevice.cs
├── TDXAirMechanics.UI/                # WPF User Interface
│   ├── Views/                         # XAML views
│   ├── ViewModels/                    # MVVM view models
│   ├── Controls/                      # Custom controls
│   └── Converters/                    # Value converters
└── TDXAirMechanics.Tests/             # Unit tests
    ├── Core.Tests/
    ├── SimConnect.Tests/
    └── DirectInput.Tests/
```

## 8. Dependency Injection Setup
```csharp
// Program.cs or App.xaml.cs
services.AddSingleton<ISimConnectManager, SimConnectManager>();
services.AddSingleton<IDirectInputManager, DirectInputManager>();
services.AddSingleton<IForceCalculationEngine, ForceCalculationEngine>();
services.AddSingleton<IConfigurationService, ConfigurationService>();
services.AddTransient<MainViewModel>();
```

## 9. Async/Await Pattern Usage
- All I/O operations (SimConnect, file operations) use async/await
- Background tasks use Task.Run for CPU-intensive operations
- CancellationTokens for graceful shutdown
- ConfigureAwait(false) for library code

## 10. Memory Management
- IDisposable implementation for native resources
- Using statements for automatic resource cleanup
- WeakReferences for event subscriptions to prevent memory leaks
- Proper disposal of DirectInput devices and effects