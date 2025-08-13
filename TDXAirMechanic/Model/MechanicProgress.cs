using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDXAirMechanic.Model
{
    public class MechanicProgress
    {
        public string Status { get; set; } = "";
        public List<string> Joysticks { get; set; } = [];
        public MechanicProgress() { }
        public MechanicProgress(string status, List<string> joysticks)
        {
            Status = status;
            Joysticks = joysticks;
        }
    }
}
