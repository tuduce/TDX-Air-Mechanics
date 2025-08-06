using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDXAirMechanic.Model
{

    // Enum to define commands FROM the UI TO SimConnect
    public enum SimCommandType
    {
        SetAutopilotHeading,
        ToggleParkingBrake,
        // Add any other commands you need
    }

    // Data structure for commands
    public class SimCommand
    {
        public SimCommandType CommandType { get; set; }
        public object Value { get; set; } // e.g., an integer for heading
    }
}
