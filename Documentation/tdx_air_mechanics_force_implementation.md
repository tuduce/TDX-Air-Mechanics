# TDX Air Mechanics - Force Implementation & Physics

## 1. DirectInput Integration with SharpDX

### 1.1 DirectInput Constants and Ranges
```csharp
public class DirectInputManager : IDirectInputManager, IDisposable
{
    private DirectInput _directInput;
    private Joystick _joystick;
    private readonly Dictionary<EffectType, Effect> _effects;
    private readonly ILogger<DirectInputManager> _logger;
    
    // DirectInput force range constants for TDX Air Mechanics
    private const int DI_FORCE_MIN = -35767;
    private const int DI_FORCE_MAX = 35767;
    private const int DI_FORCE_CENTER = 0;
    private const int DI_EFFECT_PARAM_MAX = 35767; // For duration, magnitude, etc.
    private const float SAFETY_LIMIT = 0.85f; // 85% of max force for safety
}
```

### 1.2 Initialization and Device Management
```csharp
public async Task<bool> InitializeAsync()
{
    try
    {
        _directInput = new DirectInput();
        
        // Find force-feedback capable joysticks for TDX Air Mechanics
        var joystickGuid = FindForceFeedbackJoystick();
        if (joystickGuid == Guid.Empty)
            return false;
            
        _joystick = new Joystick(_directInput, joystickGuid);
        _joystick.SetCooperativeLevel(IntPtr.Zero, 
            CooperativeLevel.Background | CooperativeLevel.Exclusive);
        
        await InitializeEffectsAsync();
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to initialize DirectInput for TDX Air Mechanics");
        return false;
    }
}

private Guid FindForceFeedbackJoystick()
{
    foreach (var deviceInstance in _directInput.GetDevices(
        DeviceType.Joystick, DeviceEnumerationFlags.ForceFeedback))
    {
        return deviceInstance.InstanceGuid;
    }
    return Guid.Empty;
}
```

### 1.3 Force Application Methods
```csharp
public async Task ApplyForceAsync(ForceVector force)
{
    if (_joystick == null || !_effects.ContainsKey(EffectType.ConstantForce))
        return;
        
    var effect = _effects[EffectType.ConstantForce];
    var parameters = new EffectParameters
    {
        Duration = int.MaxValue, // Infinite duration for TDX Air Mechanics
        Flags = EffectFlags.Cartesian | EffectFlags.ObjectOffsets
    };
    
    // Convert normalized force vector (-1.0 to 1.0) to DirectInput force range
    var constantForce = new ConstantForce
    {
        X = NormalizeForceToDirectInput(force.X),
        Y = NormalizeForceToDirectInput(force.Y),
        Z = NormalizeForceToDirectInput(force.Z)
    };
    
    parameters.Parameters = constantForce;
    effect.SetParameters(parameters, EffectParameterFlags.Start);
}

public async Task ApplyPeriodicForceAsync(float frequency, float amplitude, EffectType effectType)
{
    if (_joystick == null || !_effects.ContainsKey(effectType))
        return;
        
    var effect = _effects[effectType];
    var parameters = new EffectParameters
    {
        Duration = int.MaxValue,
        Flags = EffectFlags.Cartesian
    };
    
    var periodicForce = new PeriodicForce
    {
        Magnitude = NormalizeEffectParameter(amplitude), // 0 to 35767 range
        Period = (int)(1000000 / frequency), // Period in microseconds
        Phase = 0,
        Offset = 0 // Center position
    };
    
    parameters.Parameters = periodicForce;
    effect.SetParameters(parameters, EffectParameterFlags.Start);
}
```

### 1.4 Force Scaling and Safety
```csharp
private int NormalizeForceToDirectInput(float normalizedForce)
{
    // Clamp input to -1.0 to 1.0 range for TDX Air Mechanics safety
    normalizedForce = Math.Max(-1.0f, Math.Min(1.0f, normalizedForce));
    
    // Apply safety limit (85% of max force)
    normalizedForce *= SAFETY_LIMIT;
    
    // Convert to DirectInput force range (-35767 to 35767)
    return (int)(normalizedForce * DI_FORCE_MAX);
}

private int NormalizeEffectParameter(float normalizedValue)
{
    // For effect parameters like magnitude, duration, etc. (0 to 35767 range)
    normalizedValue = Math.Max(0.0f, Math.Min(1.0f, normalizedValue));
    return (int)(normalizedValue * DI_EFFECT_PARAM_MAX);
}

private float DirectInputForceToNormalized(int diForce)
{
    // Convert DirectInput force range back to normalized -1.0 to 1.0
    return diForce / (float)DI_FORCE_MAX;
}
```

## 2. TDX Air Mechanics Force Calculation Engine

### 2.1 Core Force Calculation Implementation
```csharp
public class TDXForceCalculationEngine : IForceCalculationEngine
{
    private readonly ILogger<TDXForceCalculationEngine> _logger;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, AircraftProfile> _aircraftProfiles;
    
    public ForceVector CalculateElevatorForce(FlightData flightData, AircraftProfile profile)
    {
        // Dynamic pressure calculation (q = 0.5 * ρ * V²) - TDX Air Mechanics physics
        var dynamicPressure = 0.5f * flightData.AirDensity * 
                            Math.Pow(flightData.TrueAirspeed, 2);
        
        // Elevator force calculation with non-linear characteristics
        var elevatorDeflection = flightData.ElevatorPosition;
        var angleOfAttack = flightData.AngleOfAttack;
        
        // Base force proportional to dynamic pressure and deflection
        var baseForce = profile.ElevatorForceCoefficient * dynamicPressure * 
                       elevatorDeflection * profile.ControlSurfaceArea;
        
        // Apply angle of attack effects (reduces effectiveness at high AoA)
        var aoaFactor = Math.Cos(angleOfAttack * Math.PI / 180.0);
        var finalForce = baseForce * aoaFactor;
        
        // Apply TDX Air Mechanics force limiting for safety
        finalForce = Math.Max(-profile.MaxElevatorForce, 
                             Math.Min(profile.MaxElevatorForce, finalForce));
        
        return new ForceVector(0, finalForce, 0);
    }
    
    public ForceVector CalculateAileronForce(FlightData flightData, AircraftProfile profile)
    {
        var dynamicPressure = 0.5f * flightData.AirDensity * 
                            Math.Pow(flightData.TrueAirspeed, 2);
        
        var aileronDeflection = flightData.AileronPosition;
        var rollRate = flightData.RollRate;
        
        // Base aileron force with TDX Air Mechanics physics model
        var baseForce = profile.AileronForceCoefficient * dynamicPressure * aileronDeflection;
        
        // Add roll damping effect (opposes roll rate)
        var dampingForce = -profile.RollDampingCoefficient * rollRate * dynamicPressure;
        
        var totalForce = baseForce + dampingForce;
        totalForce = Math.Max(-profile.MaxAileronForce, 
                             Math.Min(profile.MaxAileronForce, totalForce));
        
        return new ForceVector(totalForce, 0, 0);
    }
    
    public ForceVector CalculateRudderForce(FlightData flightData, AircraftProfile profile)
    {
        var dynamicPressure = 0.5f * flightData.AirDensity * 
                            Math.Pow(flightData.TrueAirspeed, 2);
        
        var rudderDeflection = flightData.RudderPosition;
        var yawRate = flightData.YawRate;
        var sideslipAngle = flightData.SideslipAngle;
        
        // Base rudder force from deflection - TDX calculation
        var deflectionForce = profile.RudderForceCoefficient * dynamicPressure * rudderDeflection;
        
        // Weathervaning force from sideslip
        var weathervaningForce = profile.WeathervaningCoefficient * dynamicPressure * sideslipAngle;
        
        // Yaw damping
        var dampingForce = -profile.YawDampingCoefficient * yawRate * dynamicPressure;
        
        var totalForce = deflectionForce + weathervaningForce + dampingForce;
        totalForce = Math.Max(-profile.MaxRudderForce, 
                             Math.Min(profile.MaxRudderForce, totalForce));
        
        return new ForceVector(0, 0, totalForce);
    }
}
```

### 2.2 Stall and Buffet Effects
```csharp
public ForceVector CalculateStallBuffet(FlightData flightData, AircraftProfile profile)
{
    var stallAngle = profile.StallAngle;
    var currentAoA = Math.Abs(flightData.AngleOfAttack);
    
    // Pre-stall buffet starts at 90% of stall angle in TDX Air Mechanics
    var buffetOnsetAngle = stallAngle * 0.9f;
    
    if (currentAoA > buffetOnsetAngle)
    {
        // Calculate buffet intensity (0 to 1) using TDX physics model
        var intensityRange = stallAngle - buffetOnsetAngle;
        var intensity = Math.Min(1.0f, (currentAoA - buffetOnsetAngle) / intensityRange);
        
        // Exponential intensity curve for realistic feel
        intensity = (float)Math.Pow(intensity, 2);
        
        var buffetFrequency = profile.StallBuffetFrequency;
        var buffetAmplitude = profile.StallBuffetAmplitude * intensity;
        
        // Create periodic buffet force using TDX algorithms
        var time = DateTime.Now.Ticks / 10000.0; // Convert to milliseconds
        var buffetValue = Math.Sin(2 * Math.PI * buffetFrequency * time / 1000.0);
        
        return new ForceVector(
            buffetAmplitude * buffetValue * 0.3f, // X-axis (roll)
            buffetAmplitude * buffetValue,        // Y-axis (pitch) - primary
            buffetAmplitude * buffetValue * 0.2f  // Z-axis (yaw)
        );
    }
    
    return ForceVector.Zero;
}

public ForceVector CalculateEngineVibration(FlightData flightData, AircraftProfile profile)
{
    var rpm = flightData.EngineRPM;
    var manifoldPressure = flightData.ManifoldPressure;
    
    if (rpm < profile.MinVibratingRPM)
        return ForceVector.Zero;
    
    // Base vibration frequency from RPM - TDX Air Mechanics engine model
    var baseFrequency = rpm / 60.0f; // Convert RPM to Hz
    
    // Engine firing frequency (for 4-cylinder: RPM/60 * 2)
    var firingFrequency = baseFrequency * profile.CylindersPerRevolution;
    
    // Vibration amplitude based on power setting
    var powerSetting = manifoldPressure / profile.MaxManifoldPressure;
    var amplitude = profile.BaseVibrateionAmplitude * powerSetting;
    
    // Create multi-frequency vibration using TDX algorithms
    var time = DateTime.Now.Ticks / 10000.0;
    var primaryVibration = Math.Sin(2 * Math.PI * firingFrequency * time / 1000.0);
    var secondaryVibration = Math.Sin(2 * Math.PI * baseFrequency * time / 1000.0) * 0.3;
    
    var totalVibration = primaryVibration + secondaryVibration;
    
    return new ForceVector(
        amplitude * totalVibration * 0.4f, // Roll component
        amplitude * totalVibration * 0.8f, // Pitch component (primary)
        amplitude * totalVibration * 0.3f  // Yaw component
    );
}
```

### 2.3 Environmental Effects
```csharp
public ForceVector CalculateTurbulence(FlightData flightData, AircraftProfile profile)
{
    var turbulenceIntensity = flightData.TurbulenceIntensity;
    
    if (turbulenceIntensity < 0.1f)
        return ForceVector.Zero;
    
    // Use TDX Air Mechanics Perlin noise-like algorithm for realistic turbulence
    var time = DateTime.Now.Ticks / 10000.0;
    var seed = (int)(time / 100) % 1000; // Change seed every 100ms
    
    var random = new Random(seed);
    
    // Generate turbulence components with different frequencies
    var lowFreqX = GenerateNoise(time, 0.5, random) * turbulenceIntensity;
    var lowFreqY = GenerateNoise(time + 1000, 0.5, random) * turbulenceIntensity;
    var lowFreqZ = GenerateNoise(time + 2000, 0.5, random) * turbulenceIntensity;
    
    var highFreqX = GenerateNoise(time, 2.0, random) * turbulenceIntensity * 0.3;
    var highFreqY = GenerateNoise(time + 1000, 2.0, random) * turbulenceIntensity * 0.3;
    var highFreqZ = GenerateNoise(time + 2000, 2.0, random) * turbulenceIntensity * 0.3;
    
    // Scale by airspeed (more turbulence felt at higher speeds) - TDX physics
    var airspeedFactor = Math.Min(1.0f, flightData.TrueAirspeed / 200.0f);
    
    return new ForceVector(
        (lowFreqX + highFreqX) * airspeedFactor * profile.TurbulenceSensitivity,
        (lowFreqY + highFreqY) * airspeedFactor * profile.TurbulenceSensitivity,
        (lowFreqZ + highFreqZ) * airspeedFactor * profile.TurbulenceSensitivity
    );
}

private double GenerateNoise(double time, double frequency, Random random)
{
    // TDX Air Mechanics noise generation - in production, use proper Perlin noise
    var t = time * frequency / 1000.0;
    var intT = (int)t;
    var fracT = t - intT;
    
    // Interpolate between random values
    var val1 = (random.NextDouble() - 0.5) * 2.0;
    var val2 = (random.NextDouble() - 0.5) * 2.0;
    
    return val1 * (1 - fracT) + val2 * fracT;
}
```

## 3. TDX Air Mechanics DirectInput Effects Configuration

### 3.1 Effect Types and Usage
```csharp
public enum TDXForceEffectType
{
    ConstantForce,    // Steady aerodynamic loads
    PeriodicSine,     // Engine vibration, smooth oscillations
    PeriodicSquare,   // Rough engine effects
    PeriodicRamp,     // Gradual force changes
    Spring,           // Control centering
    Damper,           // Control damping
    Friction,         // Static friction simulation
    Inertia          // Control mass simulation
}

private async Task InitializeEffectsAsync()
{
    // Constant force for steady aerodynamic loads in TDX Air Mechanics
    _effects[EffectType.ConstantForce] = CreateConstantForceEffect();
    
    // Periodic effects for vibrations
    _effects[EffectType.PeriodicSine] = CreatePeriodicEffect(PeriodicForceType.Sine);
    _effects[EffectType.PeriodicSquare] = CreatePeriodicEffect(PeriodicForceType.Square);
    
    // Condition effects for control feel
    _effects[EffectType.Spring] = CreateConditionEffect(ConditionEffectType.Spring);
    _effects[EffectType.Damper] = CreateConditionEffect(ConditionEffectType.Damper);
    _effects[EffectType.Friction] = CreateConditionEffect(ConditionEffectType.Friction);
}

private Effect CreateConstantForceEffect()
{
    var effect = new Effect(_joystick, EffectGuid.ConstantForce);
    
    var parameters = new EffectParameters
    {
        Duration = int.MaxValue,
        SamplePeriod = 0,
        Gain = DI_EFFECT_PARAM_MAX,
        Flags = EffectFlags.Cartesian | EffectFlags.ObjectOffsets,
        Axes = new int[] { 0, 1 }, // X and Y axes for TDX Air Mechanics
        Direction = new int[] { 0, 0 }
    };
    
    effect.SetParameters(parameters, EffectParameterFlags.AllParams);
    return effect;
}
```

### 3.2 TDX Effect Management and Priorities
```csharp
public class TDXEffectManager
{
    private readonly Dictionary<TDXForceEffectType, Effect> _effects;
    private readonly Dictionary<TDXForceEffectType, int> _priorities;
    private readonly object _effectLock = new object();
    
    public void SetEffectPriority(TDXForceEffectType effectType, int priority)
    {
        lock (_effectLock)
        {
            _priorities[effectType] = priority;
            
            // Recalculate effect ordering based on TDX Air Mechanics priorities
            RecalculateEffectOrdering();
        }
    }
    
    public async Task StartEffect(TDXForceEffectType effectType, ForceParameters parameters)
    {
        lock (_effectLock)
        {
            if (_effects.ContainsKey(effectType))
            {
                var effect = _effects[effectType];
                effect.SetParameters(ConvertParameters(parameters), EffectParameterFlags.Start);
                effect.Start();
            }
        }
    }
    
    public async Task StopEffect(TDXForceEffectType effectType)
    {
        lock (_effectLock)
        {
            if (_effects.ContainsKey(effectType))
            {
                _effects[effectType].Stop();
            }
        }
    }
    
    public async Task EmergencyStop()
    {
        lock (_effectLock)
        {
            foreach (var effect in _effects.Values)
            {
                effect.Stop();
            }
        }
        
        _logger.LogWarning("TDX Air Mechanics emergency stop activated - all force effects stopped");
    }
}
```

## 4. TDX Air Mechanics Safety and Limits

### 4.1 Force Limiting System
```csharp
public class TDXForceLimiter
{
    private readonly float _maxForceNewtons;
    private readonly float _safetyFactor;
    private readonly TimeSpan _maxForceTime;
    private DateTime _lastHighForceTime;
    
    public ForceVector LimitForce(ForceVector inputForce, AircraftProfile profile)
    {
        // Apply TDX Air Mechanics global force limits
        var limitedForce = new ForceVector(
            Math.Max(-_maxForceNewtons, Math.Min(_maxForceNewtons, inputForce.X)),
            Math.Max(-_maxForceNewtons, Math.Min(_maxForceNewtons, inputForce.Y)),
            Math.Max(-_maxForceNewtons, Math.Min(_maxForceNewtons, inputForce.Z))
        );
        
        // Check for sustained high forces - TDX safety system
        if (limitedForce.Magnitude > _maxForceNewtons * 0.8f)
        {
            if (DateTime.Now - _lastHighForceTime > _maxForceTime)
            {
                // Reduce force if sustained too long
                limitedForce *= 0.7f;
            }
            _lastHighForceTime = DateTime.Now;
        }
        
        // Apply aircraft-specific limits in TDX Air Mechanics
        limitedForce.X = Math.Max(-profile.MaxRollForce, 
                                 Math.Min(profile.MaxRollForce, limitedForce.X));
        limitedForce.Y = Math.Max(-profile.MaxPitchForce, 
                                 Math.Min(profile.MaxPitchForce, limitedForce.Y));
        limitedForce.Z = Math.Max(-profile.MaxYawForce, 
                                 Math.Min(profile.MaxYawForce, limitedForce.Z));
        
        return limitedForce * _safetyFactor;
    }
}
```

## 5. TDX Air Mechanics Data Models

### 5.1 Core Data Structures
```csharp
public struct ForceVector
{
    public float X { get; set; } // Roll force
    public float Y { get; set; } // Pitch force  
    public float Z { get; set; } // Yaw force
    
    public float Magnitude => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
    
    public static ForceVector Zero => new ForceVector(0, 0, 0);
    
    public ForceVector(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    
    public static ForceVector operator +(ForceVector a, ForceVector b) =>
        new ForceVector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    
    public static ForceVector operator *(ForceVector v, float scalar) =>
        new ForceVector(v.X * scalar, v.Y * scalar, v.Z * scalar);
}

public class TDXFlightData
{
    public float TrueAirspeed { get; set; }
    public float IndicatedAirspeed { get; set; }
    public float GroundSpeed { get; set; }
    public float AngleOfAttack { get; set; }
    public float SideslipAngle { get; set; }
    public float ElevatorPosition { get; set; }
    public float AileronPosition { get; set; }
    public float RudderPosition { get; set; }
    public float PitchRate { get; set; }
    public float RollRate { get; set; }
    public float YawRate { get; set; }
    public float EngineRPM { get; set; }
    public float ManifoldPressure { get; set; }
    public float TurbulenceIntensity { get; set; }
    public float AirDensity { get; set; }
    public bool OnGround { get; set; }
    public string AircraftTitle { get; set; }
    
    // TDX Air Mechanics specific properties
    public DateTime LastUpdate { get; set; }
    public bool IsValidData { get; set; }
    public string TDXVersion { get; set; }
}

public class TDXAircraftProfile
{
    public string AircraftModel { get; set; }
    public string TDXProfileVersion { get; set; }
    
    // Force coefficients for TDX Air Mechanics
    public float ElevatorForceCoefficient { get; set; }
    public float AileronForceCoefficient { get; set; }
    public float RudderForceCoefficient { get; set; }
    public float RollDampingCoefficient { get; set; }
    public float YawDampingCoefficient { get; set; }
    public float WeathervaningCoefficient { get; set; }
    
    // Physical limits
    public float MaxElevatorForce { get; set; }
    public float MaxAileronForce { get; set; }
    public float MaxRudderForce { get; set; }
    public float MaxRollForce { get; set; }
    public float MaxPitchForce { get; set; }
    public float MaxYawForce { get; set; }
    
    // Stall and buffet characteristics
    public float StallAngle { get; set; }
    public float StallBuffetFrequency { get; set; }
    public float StallBuffetAmplitude { get; set; }
    
    // Engine vibration parameters
    public float MinVibratingRPM { get; set; }
    public float CylindersPerRevolution { get; set; }
    public float BaseVibrateionAmplitude { get; set; }
    public float MaxManifoldPressure { get; set; }
    
    // Environmental sensitivity
    public float TurbulenceSensitivity { get; set; }
    public float ControlSurfaceArea { get; set; }
    
    // TDX Air Mechanics metadata
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "TDX Air Mechanics";
    public Dictionary<string, object> CustomParameters { get; set; } = new();
}
```