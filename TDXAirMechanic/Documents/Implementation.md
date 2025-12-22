# TDX Air Mechanic — Implementation Overview

Date: 2025-12-22  
Target: .NET 9, C# 13.0

## Purpose

TDX Air Mechanic provides a force feedback (FFB) mechanic layer wired to flight simulator variables. It detects compatible FFB joysticks via DirectInput, allows selecting a device, and applies effects (e.g., centered spring) based on the active airplane profile and incoming simulator data.

## Architecture

- UI Layer (`MainForm`)
  - Starts/stops services.
  - Selects active joystick.
  - Sets active airplane profile.
  - Receives progress updates for status and device lists.

- Services
  - `MechanicService` (FFB device management and effects)
  - `SimConnectService` (simulator variables producer, enqueues to `MechanicService`)

- Models
  - `AirplaneProfile`: Flags that control FFB behaviors (CenteredSpring, DynamicSpring, StickShaker, Model).
  - `SimVariableData`: Simulator values pushed to the mechanic pipeline.
  - `MechanicProgress`: Progress reporting contract (status updates, joystick names, commands).
  - `SimCommand`: Commands for simulator operations (future expansion).

## Data Flow

1. `SimConnectService` publishes `SimVariableData` into `MechanicService` via a single-producer/single-consumer channel.
2. `MechanicService` runs a background task that drains the channel and processes data.
3. Based on the `AirplaneProfile`, FFB effects are created and controlled on the selected joystick.
4. Progress is reported back to UI using `IProgress<MechanicProgress>` for thread-safe updates.

## MechanicService Details

- Lifecycle
  - `Start(IProgress<MechanicProgress>)`: Boots the background worker (`Task.Run(DoMechanicWorkAsync)`).
  - `Dispose()`: Cancels the task, waits for completion, unacquires and disposes joystick and DirectInput resources.

- Joystick Management
  - `LoadJoysticks()`: Enumerates attached game controllers and filters to FFB-capable devices using `Capabilities.Flags.HasFlag(DeviceFlags.ForceFeedback)`. Reports device names to UI via `MechanicProgress`.
  - `SelectJoystick(name, hwnd)`: Creates a `Joystick`, sets cooperative level to Exclusive|Foreground, configures properties (buffer size, autocenter off, full gain), acquires the device, stops/resets prior FFB, enumerates supported effects, and stores info text. Gracefully handles errors.

- Profiles
  - `SetActiveProfile(AirplaneProfile)`: Stores active profile used by the mechanic loop to decide which effects to apply.

- Sim Data Pipeline
  - Channel: `Channel<SimVariableData>` with `SingleReader/SingleWriter`.
  - Enqueue APIs:
    - `TryEnqueueSimData(SimVariableData)` — non-blocking.
    - `EnqueueSimDataAsync(SimVariableData, ct)` — async.
  - Worker: `DoMechanicWorkAsync()` continuously drains queued data and waits for next item. Cancellation via `_cts`.

- Effects
  - Centered Spring (initial implementation)
    - Created once when:
      - `AirplaneProfile.CenteredSpring == true`
      - An active joystick is present
      - No existing spring effect is initialized
    - Axis discovery:
      - Prefers `ForceFeedbackActuator` objects ordered by `Usage`.
      - Fallback to `Axis` objects filtered to Usage 48/49 (commonly X/Y).
    - Effect parameters:
      - `EffectFlags.ObjectOffsets | EffectFlags.Cartesian`
      - Duration: infinite (`int.MaxValue`)
      - Gain: 10000
      - Trigger: none
    - Conditions per axis:
      - Center offset 0
      - Positive/NegativeCoefficient: 10000
      - DeadBand: 500
      - Positive/NegativeSaturation: 10000
    - Starts the effect with loop count 1 (infinite).
    - Note: Currently only initialized in fallback branch; actuator-first branch is planned.

## Threading and Safety

- Single writer/reader channel for `SimVariableData` ensures minimal contention.
- UI communications use `IProgress<MechanicProgress>` to marshal updates safely.
- Joystick lifecycle guarded by `Unacquire()` + `Dispose()`.
- Cancellation token source `_cts` controls the mechanic worker. Exceptions are handled cleanly in dispose.

## Error Handling

- Robust try/catch around DirectInput operations to avoid device-specific crashes.
- Diagnostic logging via `Debug.WriteLine` for selection, enumeration, FFB checks, and worker errors.
- User-friendly messages returned from `SelectJoystick()` on failures.

## Current Limitations / TODOs

- Effect coordination:
  - Only the centered spring effect is implemented and only created under specific path; proper creation when actuators exist should be added.
  - No dynamic spring tuning (e.g., based on airspeed).
  - No stick shaker implementation.
- Profile changes do not currently reconfigure or stop/restart active effects.
- Joystick re-selection does not explicitly dispose any existing effect instances; consider tracking and stopping effects on device changes.
- Axis discovery:
  - Assumes Usage 48/49 for X/Y; needs broader mapping and validation across diverse devices.
- UI assumptions:
  - Cooperative level requires a valid, foreground `hwnd`.

## Disposal and Resource Management

- Ensures:
  - `_cts.Cancel()` and wait on `_mechanicTask`.
  - `Unacquire()` on active joystick before disposal.
  - Disposes `_cts`, `_directInput`, and `_mechanicTask`.
- Idempotent `Dispose(bool)` via `_disposed` guard.

## Next Steps

- Separate the force-feedback effects management in a separate service/class.
  - The effects service should have an interface that the mechanioc service can call into to create/update/remove effects.
  - The effects service should manage the lifecycle of effects.
  - The effects service should be able to respond to profile changes. The effects service should prepare a spring effect, two sine effects for stick shaker, etc.
- Centralize effect lifecycle:
  - Track created effects, stop/reset on profile or device changes.
- Separate profile management from mechanic service.
  - The user should b e able to save/load profiles from disk.
  - If a profile with the aircraft model name exists, it is loaded automatically.
- Add dynamic tuning of spring based on `SimVariableData` (airspeed, trim, etc.).
- Implement stick shaker effect for stall/overspeed conditions.
