# TDX Air Mechanic — Implementation Overview

Date: 2025-12-22  
Target: .NET 9, C# 13.0

## Purpose

TDX Air Mechanic provides a force feedback (FFB) mechanic layer wired to flight simulator variables. It detects compatible FFB joysticks via DirectInput, allows selecting a device, and applies effects (e.g., centered spring, stick shaker) based on the active airplane profile and incoming simulator data.

## Architecture

- UI Layer (`MainForm`)
  - Starts/stops services.
  - Selects active joystick.
  - Sets active airplane profile.
  - Receives progress updates for status and device lists.
  - Displays available profiles from disk and allows saving new profiles.

- Services
  - `MechanicService` (device management, sim data pipeline; delegates effects to `IEffectsService`).
  - `EffectsService` (implements `IEffectsService`: manages FFB effects lifecycle for the attached joystick).
  - `SimConnectService` (simulator variables producer, enqueues to `MechanicService`).
  - `ProfileManager` (implements `IProfileManager`: JSON persistence of airplane profiles, filename sanitization, listing profiles by display `Model`).

- Dependency Injection
  - Registered via `Program` using `Microsoft.Extensions.DependencyInjection`.
  - `IEffectsService` -> `EffectsService` as singleton.
  - `IProfileManager` -> `ProfileManager` as singleton.
  - `MechanicService` receives `IEffectsService` via constructor injection.

- Models
  - `AirplaneProfile`: Flags that control FFB behaviors (CenteredSpring, DynamicSpring, StickShaker, Model).
  - `SimVariableData`: Simulator values pushed to the mechanic pipeline.
  - `MechanicProgress`: Progress reporting contract (status updates, joystick names, commands).
  - `SimCommand`: Commands for simulator operations (future expansion).

## Data Flow

1. `SimConnectService` publishes `SimVariableData` into `MechanicService` via a single-producer/single-consumer channel.
2. `MechanicService` runs a background task that drains the channel and processes data.
3. `MechanicService` forwards data and profile changes to `IEffectsService`, which creates/updates effects on the selected joystick.
4. Progress is reported back to UI using `IProgress<MechanicProgress>` for thread-safe updates.
5. `MainForm` uses `IProfileManager` to load/save the current aircraft model's profile; the "Effects" tab lists available profiles from disk.

## MechanicService Details

- Lifecycle
  - `Start(IProgress<MechanicProgress>)`: Boots the background worker (`Task.Run(DoMechanicWorkAsync)`).
  - `Dispose()`: Cancels the task, waits for completion, unacquires and disposes joystick and DirectInput resources, resets and disposes effects.

- Joystick Management
  - `LoadJoysticks()`: Enumerates attached game controllers and filters to FFB-capable devices using `Capabilities.Flags.HasFlag(DeviceFlags.ForceFeedback)`. Reports device names to UI via `MechanicProgress`.
  - `SelectJoystick(name, hwnd)`: Creates a `Joystick`, sets cooperative level to Exclusive|Background (so effects persist when app loses focus), configures properties (buffer size, autocenter off, full gain), acquires the device, stops/resets prior FFB, attaches device to `IEffectsService`, and applies current profile. Enumerates supported effects for info text.

- Profiles
  - `SetActiveProfile(AirplaneProfile)`: Calls `IEffectsService.ApplyProfile(profile)`.

- Sim Data Pipeline
  - Channel: `Channel<SimVariableData>` with `SingleReader/SingleWriter`.
  - Enqueue APIs:
    - `TryEnqueueSimData(SimVariableData)` — non-blocking.
    - `EnqueueSimDataAsync(SimVariableData, ct)` — async.
  - Worker: `DoMechanicWorkAsync()` continuously drains queued data and forwards to `IEffectsService.Update(data)`. Cancellation via `_cts`.

## EffectsService Details

- Responsibilities
  - Attach/detach the current `Joystick` and manage the lifecycle of effects.
  - Apply `AirplaneProfile` to enable/disable effects.
  - Update effects in response to `SimVariableData`.
  - Reset and clean up effects when device changes or disposing.

- Centered Spring
  - Created when profile has `CenteredSpring == true` and a joystick is attached.
  - Axis discovery:
    - Prefers `ForceFeedbackActuator` objects ordered by `Usage`.
    - Fallback to `Axis` objects filtered to Usage 48/49 (X/Y).
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
  - Starts with loop count 1 (infinite). Properly stopped/disposed on removal.

- Stick Shaker
  - Enabled when `profile.StickShaker == true`.
  - Trigger conditions:
    - Stall: `StallWarning > 0.5`.
    - Overspeed: near barber pole (`IAS >= Barber * 0.98`).
  - Implemented with a periodic effect (`PeriodicForce`) on X/Y axes.
  - Magnitude/period tuned based on stall vs overspeed; parameters updated in-place with `SetParameters`.

- Threading and Disposal Safety
  - Uses safe joystick access (`NativePointer` guard) to avoid calling DirectInput on disposed devices during shutdown.
  - Captures a local joystick reference for effect creation/update to avoid races.

## Threading and Safety

- Single writer/reader channel for `SimVariableData` ensures minimal contention.
- UI communications use `IProgress<MechanicProgress>` to marshal updates safely.
- Joystick lifecycle guarded by `Unacquire()` + `Dispose()` in `MechanicService`.
- `EffectsService` cleans up effects on device detach/dispose and guards against disposed device access.
- Cancellation token source `_cts` controls the mechanic worker. Exceptions are handled cleanly in dispose.

## Error Handling

- Robust try/catch around DirectInput operations to avoid device-specific crashes.
- Diagnostic logging via `Debug.WriteLine` for selection, enumeration, FFB checks, worker errors, and effect creation/update.
- User-friendly messages returned from `SelectJoystick()` on failures.

## Current Limitations / TODOs

- Dynamic spring tuning (e.g., based on airspeed) not yet implemented.
- Axis discovery assumes Usage 48/49 for fallback; needs broader validation.
- Effect parameterization is static; expose tuning via profile settings.
- Profile changes do not yet adjust spring coefficients or shaker frequency beyond enable/disable.

## Disposal and Resource Management

- Ensures:
  - `_cts.Cancel()` and wait on `_mechanicTask`.
  - `Unacquire()` on active joystick before disposal.
  - Resets and disposes effects via `IEffectsService`.
  - Disposes `_cts`, `_directInput`, and `_mechanicTask`.
- Idempotent `Dispose(bool)` via `_disposed` guard.

## Next Steps

- Add dynamic tuning of spring based on `SimVariableData` (airspeed, trim, etc.).
- Enhance axis mapping and normalization across devices.
- Improve progress reporting granularity (errors, device capability summaries).
