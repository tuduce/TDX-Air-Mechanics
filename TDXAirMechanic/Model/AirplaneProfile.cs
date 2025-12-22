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

        // Trim enable state (UI `TrimSwitch`); trim acts only if CenteredSpring is also enabled.
        public bool TrimEnabled { get; set; } = true;

        // Trim button mappings (indices from joystick button array)
        // When set to -1, the mapping is disabled
        public int PitchTrimUpButton { get; set; } = -1;
        public int PitchTrimDownButton { get; set; } = -1;
        public int RollTrimLeftButton { get; set; } = -1;
        public int RollTrimRightButton { get; set; } = -1;

        // Trim step in device units (Effect Condition Offset). Small increments per press.
        // Typical DirectInput range is device-dependent; start conservatively.
        public int TrimStep { get; set; } = 200; // per press

        // Optional limits to avoid excessive trim shifts
        public int MaxTrimOffset { get; set; } = 4000; // absolute cap per axis
    }
}
