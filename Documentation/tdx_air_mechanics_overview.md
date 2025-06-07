# TDX Air Mechanics - Project Overview & Requirements

## 1. Executive Summary

### 1.1 Project Overview
TDX Air Mechanics is a Windows application that bridges Microsoft Flight Simulator (MSFS2020/2024) with force-feedback joysticks to provide realistic tactile feedback during flight simulation. The program uses SimConnect SDK to gather real-time flight data and translates aerodynamic forces into appropriate force-feedback effects.

### 1.2 Target Users
- Flight simulation enthusiasts with force-feedback joysticks
- Virtual pilots seeking enhanced immersion
- Users of legacy force-feedback hardware (Microsoft SideWinder, Logitech, etc.)

### 1.3 Key Benefits
- Enhanced flight realism through tactile feedback
- Support for multiple force-feedback joystick models
- Real-time response to flight dynamics
- Customizable force profiles for different aircraft

## 2. System Requirements

### 2.1 Minimum System Requirements
- **OS**: Windows 10 (64-bit) or Windows 11
- **CPU**: Intel i5-4590 / AMD FX 8350 or equivalent
- **RAM**: 8 GB minimum, 16 GB recommended
- **Storage**: 100 MB available space
- **Dependencies**: 
  - Microsoft Flight Simulator 2020 or 2024
  - DirectX 11 or higher
  - Visual C++ Redistributable 2019 or later

### 2.2 Supported Hardware
- Force-feedback joysticks with DirectInput support
- Primary targets: Microsoft SideWinder series, Logitech Force 3D Pro, Thrustmaster force-feedback models
- USB 2.0 or higher connection

## 3. Functional Requirements

### 3.1 Core Features
- **SimConnect Integration**: Real-time data acquisition from MSFS
- **Force Calculation Engine**: Convert flight data to force vectors
- **DirectInput Interface**: Send force-feedback commands to joystick
- **Configuration System**: User-customizable force profiles
- **Aircraft Detection**: Automatic aircraft-specific force profiles
- **Real-time Monitoring**: Live display of forces and system status

### 3.2 Force-Feedback Effects
- **Aerodynamic Forces**:
  - Control surface loading (elevator, aileron, rudder)
  - Airspeed-dependent stick forces
  - Stall buffet and pre-stall warning
  - Ground effect simulation
- **Engine Effects**:
  - Propeller gyroscopic effects
  - Engine vibration
  - Asymmetric thrust (multi-engine)
- **Environmental Effects**:
  - Turbulence simulation
  - Wind shear effects
  - Ground contact forces (tailwheel shimmy, runway rumble)

### 3.3 Data Sources (SimConnect Variables)
- Airspeed (indicated, true, ground speed)
- Control surface positions and forces
- Aircraft attitude (pitch, bank, yaw)
- Engine parameters (RPM, torque, thrust)
- Environmental data (wind, turbulence)
- Landing gear status
- Structural G-forces

## 4. Performance Requirements
- **Update Rate**: 60-120 Hz for smooth force feedback
- **Latency Target**: <16ms from data to force output
- **Memory Usage**: <50MB typical operation
- **CPU Usage**: <5% on recommended hardware

## 5. Success Metrics

### 5.1 Technical Metrics
- **Latency**: <16ms average response time
- **Stability**: >99% uptime during normal operation
- **Performance**: <5% CPU usage on recommended hardware
- **Compatibility**: Support for 80%+ of common FF joysticks

### 5.2 User Satisfaction Metrics
- **User Feedback**: Regular community surveys
- **Adoption Rate**: Download and active user tracking
- **Bug Reports**: Defect rate trending
- **Feature Requests**: Community engagement level

## 6. Risk Assessment

### 6.1 Technical Risks
- **SimConnect Changes**: Microsoft SDK modifications
- **Hardware Compatibility**: Legacy device support challenges
- **Performance Issues**: System resource constraints
- **Driver Dependencies**: Third-party driver requirements

### 6.2 Mitigation Strategies
- **SDK Versioning**: Support multiple SimConnect versions
- **Hardware Abstraction**: Modular hardware interface layer
- **Performance Monitoring**: Real-time performance tracking
- **Fallback Modes**: Graceful degradation capabilities