using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDXAirMechanic.Model
{
    public class AirplaneProfile
    {
        public string Model { get; set; } = "";

        // Effects profile (per-aircraft)
        public bool CenteredSpring { get; set; }
        public bool DynamicSpring { get; set; }
        public bool StickShaker { get; set; }
        public bool GearVibration { get; set; } // Vibrate stick when landing gear is lowered
        public bool GroundVibration { get; set; } // Ground roll vibrations when on ground

        // Helicopter cyclic behavior (mutually exclusive with CenteredSpring)
        public bool CyclicEnabled { get; set; } = false;
        // 0..100 UI values mapped to coefficients
        public int CyclicDamping { get; set; } = 0;   // maps to damper coefficient
        public int CyclicSpring { get; set; } = 0;    // maps to spring coefficient

        // Trim enable state (UI `TrimSwitch`); trim acts only if CenteredSpring is also enabled.
        public bool TrimEnabled { get; set; } = true;

        // Trim button mappings (indices from joystick button array)
        // When set to -1, the mapping is disabled
        public int PitchTrimUpButton { get; set; } = -1;
        public int PitchTrimDownButton { get; set; } = -1;
        public int RollTrimLeftButton { get; set; } = -1;
        public int RollTrimRightButton { get; set; } = -1;

        // Additional buttons for cyclic UX
        public int TrimResetButton { get; set; } = -1;       // re-center trim offsets
        public int TrimDisconnectButton { get; set; } = -1;  // temporarily disable spring while held

        // Trim step in device units (Effect Condition Offset). Small increments per press.
        // Typical DirectInput range is device-dependent; start conservatively.
        public int TrimStep { get; set; } = 100; // per press

        // Optional limits to avoid excessive trim shifts
        public int MaxTrimOffset { get; set; } = 4000; // absolute cap per axis
    }
}
