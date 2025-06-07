# TDX Air Mechanics

A Windows application that bridges Microsoft Flight Simulator (MSFS 2020/2024) with force-feedback joysticks to provide realistic tactile feedback during flight simulation.

## Overview

TDX Air Mechanics uses the SimConnect SDK to gather real-time flight data and translates aerodynamic forces into appropriate force-feedback effects on compatible joysticks. This enhances the flight simulation experience by providing realistic control surface loading, stall buffet, turbulence effects, and more.

## Features

- **Real-time SimConnect Integration**: Live data acquisition from Microsoft Flight Simulator
- **Force Feedback Effects**: 
  - Aerodynamic control surface loading
  - Stall buffet and warning vibrations
  - Turbulence simulation
  - Ground effect forces
  - Engine-induced vibrations
- **Aircraft-Specific Profiles**: Customizable force profiles for different aircraft types
- **Safety Features**: Force limiting and smoothing for safe operation
- **Modern UI**: Windows Forms application with system tray support
- **Configurable Settings**: Adjustable force multipliers and device settings

## System Requirements

- **OS**: Windows 10 (64-bit) or Windows 11
- **CPU**: Intel i5-4590 / AMD FX 8350 or equivalent
- **RAM**: 8 GB minimum, 16 GB recommended
- **Dependencies**: 
  - Microsoft Flight Simulator 2020 or 2024
  - .NET 8.0 Runtime
  - Visual C++ Redistributable 2019 or later

## Supported Hardware

- Force-feedback joysticks with DirectInput support
- Microsoft SideWinder series
- Logitech Force 3D Pro
- Thrustmaster force-feedback models
- Other DirectInput-compatible force feedback devices

## Building from Source

### Prerequisites

- Visual Studio 2022 or Visual Studio Code
- .NET 8.0 SDK
- Microsoft Flight Simulator SDK (for SimConnect - optional)

### Build Steps

1. Clone the repository
```bash
git clone https://github.com/your-username/TDX-Air-Mechanics.git
cd TDX-Air-Mechanics
```

2. Build the solution
```bash
dotnet build
```

3. Run the application
```bash
dotnet run --project src/TDXAirMechanics.UI
```

**Note**: The application will build and run without the MSFS SDK installed, but SimConnect features will be disabled.

## Usage

1. **Start the Application**: Launch TDXAirMechanics.UI.exe
2. **Connect Force Feedback Device**: 
   - Go to the "Devices" tab
   - Click "Refresh Devices"
   - Select your force feedback joystick
   - Click "Select Device"
3. **Configure Force Settings**:
   - Adjust force multiplier (0-200%)
   - Enable/disable force feedback
   - Modify settings for your preference
4. **Start Flight Simulator**: Launch MSFS and load any aircraft
5. **Monitor Status**: Check the "Status" tab for connection status

### Force Settings

- **Force Multiplier**: Controls overall force strength (0-200%)
- **Enable Force Feedback**: Master on/off switch
- **Safety Limits**: Automatic force limiting for safe operation

### Aircraft Profiles

The application supports aircraft-specific force profiles stored in:
```
%APPDATA%/TDXAirMechanics/aircraft-profiles.json
```

Default profiles are created for common aircraft types.
3. Restore NuGet packages
4. Build the solution (F6)

### Project Structure

```
TDXAirMechanics/
├── src/
│   ├── TDXAirMechanics.Core/           # Core domain models and interfaces
│   ├── TDXAirMechanics.UI/             # Windows Forms application
│   ├── TDXAirMechanics.SimConnect/     # SimConnect integration
│   └── TDXAirMechanics.DirectInput/    # DirectInput force feedback
├── tests/
│   └── TDXAirMechanics.Tests/          # Unit tests
└── Documentation/                       # Project documentation
```

## Configuration

The application stores configuration files in:
`%APPDATA%\TDXAirMechanics\`

- `config.json` - Main application settings
- `aircraft-profiles.json` - Aircraft-specific force profiles

## Usage

1. Start Microsoft Flight Simulator
2. Launch TDX Air Mechanics
3. Connect your force feedback joystick
4. The application will automatically detect MSFS and begin providing force feedback
5. Adjust settings and aircraft profiles as needed

## Development

### Architecture

The application follows a modular architecture with dependency injection:

- **Core**: Domain models, interfaces, and business logic
- **UI**: Windows Forms user interface and application hosting
- **SimConnect**: Integration with Microsoft Flight Simulator
- **DirectInput**: Force feedback joystick control

### Key Interfaces

- `ISimConnectManager` - SimConnect data acquisition
- `IDirectInputManager` - Force feedback device control
- `IForceCalculationEngine` - Flight data to force conversion
- `IConfigurationManager` - Settings and profile management

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

See LICENSE file for details.

## Acknowledgments

- Microsoft Flight Simulator SDK
- SharpDX DirectInput library
- Flight simulation community for testing and feedback
