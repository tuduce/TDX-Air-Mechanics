using System;

namespace TDXAirMechanic.Model
{
    // Data coming FROM SimConnectService TO MechanicService
    public class SimVariableData
    {
        public string Title { get; set; } = string.Empty;
        public double IndicatedAltitude { get; set; }
        public double IndicatedAirspeed { get; set; }
    }
}
