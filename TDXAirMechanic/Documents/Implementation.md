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

- Services
  - `MechanicService` (device management, sim data pipeline; delegates effects to `IEffectsService`).
  - `EffectsService` (implements `IEffectsService`: manages FFB effects lifecycle for the attached joystick).
  - `SimConnectService` (simulator variables producer, enqueues to `MechanicService`).

- Dependency Injection
  - Registered via `Program` using `Microsoft.Extensions.DependencyInjection`.
  - `IEffectsService` -> `EffectsService` as singleton.
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

## MechanicService Details

- Lifecycle
  - `Start(IProgress<MechanicProgress>)`: Boots the background worker (`Task.Run(DoMechanicWorkAsync)`).
  - `Dispose()`: Cancels the task, waits for completion, unacquires and disposes joystick and DirectInput resources, resets and disposes effects.

- Joystick Management
  - `LoadJoysticks()`: Enumerates attached game controllers and filters to FFB-capable devices using `Capabilities.Flags.HasFlag(DeviceFlags.ForceFeedback)`. Reports device names to UI via `MechanicProgress`.
  - `SelectJoystick(name, hwnd)`: Creates a `Joystick`, sets cooperative level to Exclusive|Foreground, configures properties (buffer size, autocenter off, full gain), acquires the device, stops/resets prior FFB, attaches device to `IEffectsService`, and re-applies current profile. Enumerates supported effects for info text.

- Profiles
  - `SetActiveProfile(AirplaneProfile)`: Stores active profile and calls `IEffectsService.ApplyProfile(profile)`.

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

## Threading and Safety

- Single writer/reader channel for `SimVariableData` ensures minimal contention.
- UI communications use `IProgress<MechanicProgress>` to marshal updates safely.
- Joystick lifecycle guarded by `Unacquire()` + `Dispose()` in `MechanicService`.
- `EffectsService` cleans up effects on device detach/dispose.
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

- Persist profiles to disk; auto-load by aircraft model name.
  - Profiles are saved in the user's AppData folder, in the local profile in a folder TDX-AirMechanic.
  - Profiles are saved as json files
  - The filename is the aircraft model name with invalid filename characters replaced with underscores.
  - There is a profile called default for the situations where a plane model is not yet known (e.g., not connected yet to a simulator)
  - When a new model name is seen the profile for that model is loaded if it exists, otherwise the default profile is used.
  - When a change is made to the profile in the UI it is saved to disk automatically.
  - On the "Effects" tab there is a dropdown to select a profile from the ones available in the profiles folder.
  - On the "Effects" tab there is now a "Save New Profile" button that saves the current profile to disk for the current aircraft model name.
  - All the UI controls are taken from the MaterialSkin library. 
- Add dynamic tuning of spring based on `SimVariableData` (airspeed, trim, etc.).
- Enhance axis mapping and normalization across devices.
- Improve progress reporting granularity (errors, device capability summaries).
