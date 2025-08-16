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
    }
}
