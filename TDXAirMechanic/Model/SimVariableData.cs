using System;

namespace TDXAirMechanic.Model
{
    // Data coming FROM SimConnectService TO MechanicService
    public class SimVariableData
    {
        public string Title { get; set; } = string.Empty;

        public double IAS { get; set; }
        public double Barber { get; set; }
        public double Throttle { get; set; }
        public double StallWarning { get; set; }
        public double OnGround { get; set; }
        public double GroundType { get; set; }
        public double GroundSpeed { get; set; }
    }
}
