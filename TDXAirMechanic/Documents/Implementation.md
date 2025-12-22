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
  - `AirplaneProfile`:
    - Identity: `Model`.
    - Effects: `CenteredSpring`, `DynamicSpring`, `StickShaker`.
    - Trim: `TrimEnabled`, `PitchTrimUpButton`, `PitchTrimDownButton`, `RollTrimLeftButton`, `RollTrimRightButton` (button indices, -1 disabled), `TrimStep` (device units per nudge), `MaxTrimOffset` (absolute clamp per axis).
  - `SimVariableData`: Simulator values pushed to the mechanic pipeline.
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
    - Center offset 0 (updated by trim; see below)
    - Positive/NegativeCoefficient: 10000 (updated by dynamic spring)
    - DeadBand: 500 (base; may be overridden in dynamic updates)
    - Positive/NegativeSaturation: 10000
  - Starts with loop count 1 (infinite). Properly stopped/disposed on removal.

- Dynamic Spring
  - Enabled only when both `CenteredSpring` and `DynamicSpring` are true in the active profile.
  - Stiffness is updated continuously based on airspeed from `SimVariableData`:
    - Normalized factor = clamp(`IAS` / `Barber`, 0..1). If `Barber <= 0`, fallback uses 250 KIAS as the max reference.
    - Coefficients (`PositiveCoefficient`/`NegativeCoefficient`) are mapped linearly from 1000 (soft at low speed) to 10000 (firm at barber-pole speed).
    - Updates are throttled: coefficients are re-applied only when the change is >= 100 to reduce DirectInput churn.
  - Implementation details:
    - Reuses the existing spring effect via `SetParameters` to update the `ConditionSet` in place; axes are preserved.
    - Axis offsets used by the spring are cached on creation and reused for updates; caches are cleared on device or effect removal.
    - Trim offsets (see below) are preserved on every dynamic update.

- Stick Shaker
  - Enabled when `profile.StickShaker == true`.
  - Trigger conditions:
    - Stall: `StallWarning > 0.5`.
    - Overspeed: near barber pole (`IAS >= Barber * 0.98`).
  - Implemented with a periodic effect (`PeriodicForce`) on X/Y axes.
  - Magnitude/period tuned based on stall vs overspeed; parameters updated in-place with `SetParameters`.

- Trim
  - API: `IEffectsService.NudgeTrim(int pitchDelta, int rollDelta)`.
  - Logic:
    - Maintains per-axis spring center offsets (`_trimOffsetX`/`_trimOffsetY`) and clamps to `MaxTrimOffset`.
    - Applies offsets to the spring effect via the `Condition.Offset` for X/Y axes (usages 48/49).
    - Only active when a device is attached and `CenteredSpring` is enabled.
  - Integration:
    - `MechanicService` calls `NudgeTrim` on mapped button presses (with auto-repeat).
    - Works alongside dynamic spring updates; offsets are preserved on stiffness changes.

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

## Recent Changes

- Trim feature implemented end-to-end:
  - UI: visibility toggling with `CenteredSpring`, joystick button capture into textboxes, persisted to profile JSON.
  - Model: added `TrimEnabled`, four trim button indices, `TrimStep`, `MaxTrimOffset`.
  - Mechanic: polls joystick at ~50 Hz, rising-edge detection with 100 ms auto-repeat for held buttons, `BeginButtonCapture/CancelButtonCapture` for UI mapping.
  - Effects: `NudgeTrim` applies spring center offsets per axis and preserves them across dynamic updates.
- Dynamic spring tuning updated:
  - Min stiffness set to 1000 (from 2000 in earlier docs), max 10000, with 100-unit update threshold.
- Profile selection and saving behavior:
  - Auto-select the aircraft model profile on simulator connect.
  - Edits save to the currently selected profile file (no implicit model overrides).
  - "Save New Profile" targets the active simulator model when available; otherwise uses the selected profile name.

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

- Expose dynamic spring tuning parameters in profiles:
  - Configure min/max stiffness and update threshold.
  - Optionally support non-linear curves (e.g., quadratic or piecewise) and separate per-axis gains.
- Expose trim parameters in profiles:
  - `TrimStep` and `MaxTrimOffset` adjustments via UI.
  - Add "Reset Trim" button to re-center offsets.
- Enhance axis mapping and normalization across devices.
- Improve progress reporting granularity (errors, device capability summaries).
