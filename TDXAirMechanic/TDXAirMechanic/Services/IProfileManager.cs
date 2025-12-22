using System.Collections.Generic;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services
{
    public interface IProfileManager
    {
        // Returns all available profile names (without extension), including "default" if present
        IReadOnlyList<string> ListProfiles();

        // Loads a profile by aircraft model name, falling back to default if not found
        AirplaneProfile LoadProfileForModel(string? modelName);

        // Saves the given profile using its Model as filename (sanitized). If Model is null/empty, saves as default.
        void SaveProfile(AirplaneProfile profile);

        // Returns the path to the profiles folder under AppData\Local
        string GetProfilesFolderPath();
    }
}
