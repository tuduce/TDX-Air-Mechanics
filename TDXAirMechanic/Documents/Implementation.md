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
  - Trim UX:
    - Shows trim controls only when `CenteredSpring` is enabled.
    - Assigns joystick buttons to trim by clicking a textbox and pressing a joystick button (live capture).
    - Persists trim enable state and mappings to the selected profile JSON.
    - Auto-selects the aircraft model profile when the simulator connects and reports a new model.
    - Saving a new profile uses the active simulator model name when available; otherwise uses the currently selected profile name.
  - Gear vibration UX:
    - Toggle `Gear Vibrations` in the Effects tab to enable/disable vibration when gear is down.
    - Persisted per aircraft in the profile JSON via `GearVibration`.
  - Ground vibration UX (new):
    - Toggle `Ground Vibrations` in the Effects tab to enable/disable ground roll vibrations.
    - Persisted per aircraft in the profile JSON via `GroundVibration`.
    - Active only while `OnGround >= 0.5`; disabled automatically when airborne.
  - Flight state UX:
    - `FlightStatusLabel` displays current state ("No Flight Loaded" or "Flight Loaded - Effects Active").

- Services
  - `MechanicService` (device management, sim data pipeline; delegates effects to `IEffectsService`).
  - `EffectsService` (implements `IEffectsService`: orchestrates lifecycle of effect classes for the attached joystick).
  - `SimConnectService` (simulator variables producer, enqueues to `MechanicService`).
  - `ProfileManager` (implements `IProfileManager`: JSON persistence of airplane profiles, filename sanitization, listing profiles by display `Model`).

- Dependency Injection
  - Registered via `Program` using `Microsoft.Extensions.DependencyInjection`.
  - `IEffectsService` -> `EffectsService` as singleton.
  - `IProfileManager` -> `ProfileManager` as singleton.
  - `MechanicService` receives `IEffectsService` via constructor injection.

- Models
  - `AirplaneProfile`:
    - Identity: `Model`.
    - Effects: `CenteredSpring`, `DynamicSpring`, `StickShaker`, `GearVibration`, `GroundVibration`.
    - Trim: `TrimEnabled`, `PitchTrimUpButton`, `PitchTrimDownButton`, `RollTrimLeftButton`, `RollTrimRightButton` (button indices, -1 disabled), `TrimStep` (device units per nudge), `MaxTrimOffset` (absolute clamp per axis).
  - `SimVariableData`: Simulator values pushed to the mechanic pipeline. Includes `IAS`, `Barber`, `OnGround`, `GroundType`, `GroundSpeed`, and `GearPosition` (0..1).
  - `MechanicProgress`: Progress reporting contract (status updates, joystick names, commands).
  - `SimCommand`: Commands for simulator operations (future expansion).

## Data Flow

1. `SimConnectService` publishes `SimVariableData` into `MechanicService` via a single-producer/single-consumer channel.
2. `MechanicService` runs a background task that drains the channel and processes data.
3. `MechanicService` forwards data and profile changes to `IEffectsService`, which creates/updates effects on the selected joystick.
4. Progress is reported back to UI using `IProgress<MechanicProgress>` for thread-safe updates.
5. `MainForm` uses `IProfileManager` to load/save the current profile; the "Effects" tab lists available profiles from disk.

## MechanicService Details

- Lifecycle
  - `Start(IProgress<MechanicProgress>)`: Boots the background worker (`Task.Run(DoMechanicWorkAsync)`).
  - `Dispose()`: Cancels the task, waits for completion, unacquires and disposes joystick and DirectInput resources, resets and disposes effects.

- Joystick Management
  - `LoadJoysticks()`: Enumerates attached game controllers and filters to FFB-capable devices using `Capabilities.Flags.HasFlag(DeviceFlags.ForceFeedback)`. Reports device names to UI via `MechanicProgress`.
  - `SelectJoystick(name, hwnd)`: Creates a `Joystick`, sets cooperative level to Exclusive|Background (so effects persist when app loses focus), configures properties (buffer size, autocenter off, full gain), acquires the device, stops/resets prior FFB, attaches device to `IEffectsService`, and applies current profile. Enumerates supported effects for info text.

- Profiles
  - `SetActiveProfile(AirplaneProfile)`: Calls `IEffectsService.ApplyProfile(profile)` and tracks the active profile for trim inputs.

- Sim Data + Input Pipeline
  - Channel: `Channel<SimVariableData>` with `SingleReader/SingleWriter`.
  - Enqueue APIs:
    - `TryEnqueueSimData(SimVariableData)` — non-blocking.
    - `EnqueueSimDataAsync(SimVariableData, ct)` — async.
  - Worker:
    - `DoMechanicWorkAsync()` drains queued data and processes UI/joystick input.
    - Between reads, polls joystick buttons roughly at 50 Hz (20 ms delay) to handle trim and button capture even when no sim data is flowing.

- Trim Button Handling
  - Rising-edge detection and auto-repeat:
    - On first press, issues one trim nudge.
    - While held, repeats the nudge every 100 ms until released.
  - Mappings come from the active profile (`PitchTrimUp/Down`, `RollTrimLeft/Right`).
  - Trim inputs are ignored unless `profile.TrimEnabled == true`.
  - UI button capture support:
    - `BeginButtonCapture(Action<int>)`/`CancelButtonCapture()` allow the UI to request the next joystick button index (rising edge) to populate a textbox.

## EffectsService Details

- Responsibilities
  - Attach/detach the current `Joystick` and manage the lifecycle of effects.
  - Apply `AirplaneProfile` to enable/disable effects.
  - Update effects in response to `SimVariableData`.
  - Reset and clean up effects when device changes or disposing.

### Effects Class Refactor (Implemented)

Effects were refactored into separate classes under `Services/Effects`, each owning its lifecycle and parameters. `EffectsService` now orchestrates these classes.

- `EffectsService`
  - Holds instances of `SpringEffect`, `StickShakerEffect`, `GearVibrationEffect`, `GroundVibrationEffect`.
  - Delegates `AttachDevice`, `DetachDevice`, `ApplyProfile`, `Update`, and `ResetAll` to the respective effect classes.
  - Delegates `NudgeTrim` to `SpringEffect`.

- `SpringEffect`
  - Implements centered spring creation and dynamic stiffness updates.
  - Maintains trim offsets per axis and applies via `Condition.Offset`.
  - Axis discovery prefers `ForceFeedbackActuator`, falls back to X/Y usages 48/49.
  - Preserves offsets across dynamic updates and throttles stiffness changes (<100 diff).

- `StickShakerEffect`
  - Periodic sine effect on X/Y axes for stall/overspeed.
  - Lazily created, updated in-place based on conditions.

- `GearVibrationEffect`
  - Two periodic sine effects when gear is down and aircraft is airborne.
  - Magnitude scales with `IAS` normalized by `Barber` (or 250 KIAS fallback).

- `GroundVibrationEffect`
  - Two periodic layers plus optional `ConstantForce` pulses on concrete.
  - Active only while `OnGround >= 0.5`.
  - Surface mapping values (MSFS SDK):
    - Concrete = 0
    - Grass = 1
    - Asphalt = 4
    - Tarmac = 17
  - Behavior: asphalt (minimal high-frequency), grass (low-frequency increasing with speed), concrete (background + jolts every ~20 m).

All classes implement: `AttachDevice`, `DetachDevice`, `ApplyProfile`, `Update`, `Reset`, `Start`, `Stop`, `Dispose`.

## Flight State Handling

- Overview
  - Effects are active only when a flight is loaded. When no flight is loaded (menu or disconnect), all effects are disabled and cleared.

- Service Integration
  - `SimConnectService` polls SimConnect system state via `RequestSystemState("Sim")` on a 1-second interval and on initial connect.
  - `OnRecvSystemState` evaluates `dwInteger` to determine running/flight state and calls `MechanicService.SetFlightLoaded(bool)` accordingly.
  - On `Disconnect()`: resets flight state to not loaded and reports "No Flight Loaded".

- Mechanic Gating
  - `MechanicService` maintains `_effectsEnabled` which mirrors flight state.
  - When `_effectsEnabled == false`:
    - `ProcessSimData` returns without updating effects.
    - `PollTrimButtons` returns early, ignoring input.
    - `SetActiveProfile` stores the profile but defers `ApplyProfile`.
    - `SetFlightLoaded(false)` immediately calls `_effects.ResetAll()` to stop and clear all effects.
  - When `_effectsEnabled == true`:
    - `SetFlightLoaded(true)` re-applies the current profile to create effects.
    - `ProcessSimData` and `PollTrimButtons` operate normally.

- UI Feedback
  - `MainForm` wires a `Progress<MechanicProgress>` to `MechanicProgressReporter`.
  - Handles `MechanicProgressCommand.SetFlightStatus` by updating `FlightStatusLabel`.
  - Initializes `FlightStatusLabel` to "No Flight Loaded" on form load.

- Idempotency
  - State changes are handled safely and do not duplicate effect creation or clearing.

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

## Recent Changes

- Flight state detection updated:
  - Removed `SimStart/SimStop` subscriptions.
  - Added polling of SimConnect system state (`RequestSystemState("Sim")`) and gating logic in `OnRecvSystemState`.
  - Keeps idempotent status updates via `MechanicProgressCommand.SetFlightStatus`.
- Ground vibration implemented:
  - UI toggle `Ground Vibrations` bound to `AirplaneProfile.GroundVibration` and persisted per aircraft.
  - Active only while on ground; stops automatically when airborne.
  - Asphalt: minimal high-frequency vibrations using two small sine waves; magnitude scales with ground speed and is 0 when stationary.
  - Grass: moderate low-frequency vibrations using two sine waves; frequency increases with ground speed; 0 when stationary.
  - Concrete: light background sine vibration plus small jolts every 20 meters traveled; distance computed from `GroundSpeed` accumulation; jolts rendered via short `ConstantForce` pulses scaled with speed.
- Effects refactor implemented:
  - Introduced `Services/Effects` with `SpringEffect`, `StickShakerEffect`, `GearVibrationEffect`, `GroundVibrationEffect`.
  - `EffectsService` now orchestrates these classes and delegates `NudgeTrim` to `SpringEffect`.

## Current Limitations / TODOs

- Axis discovery assumes Usage 48/49 for fallback; needs broader validation.
- Effect parameterization is static; expose tuning via profile settings (e.g., min/max spring coefficients, non-linear response curve).
- Profile changes do not yet adjust shaker frequency beyond enable/disable.
- Consider exposing trim units in degrees and calibrating device units per-axis per-device.
- Add UI affordances to reset trim to center and to adjust `TrimStep`/`MaxTrimOffset` per profile.

## Disposal and Resource Management

- Ensures:
  - `_cts.Cancel()` and wait on `_mechanicTask`.
  - `Unacquire()` on active joystick before disposal.
  - Resets and disposes effects via `IEffectsService`.
  - Disposes `_cts`, `_directInput`, and `_mechanicTask`.
- Idempotent `Dispose(bool)` via `_disposed` guard.

## Next Steps

- Display the force on the 2 joystick axes in the dashboard
- Expose dynamic spring tuning parameters in profiles:
  - Configure min/max stiffness and update threshold.
  - Optionally support non-linear curves (e.g., quadratic or piecewise) and separate per-axis gains.
- Expose trim parameters in profiles:
  - `TrimStep` and `MaxTrimOffset` adjustments via UI.
  - Add "Reset Trim" button to re-center offsets.
