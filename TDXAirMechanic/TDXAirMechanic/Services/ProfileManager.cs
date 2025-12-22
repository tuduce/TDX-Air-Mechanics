using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using TDXAirMechanic.Model;

namespace TDXAirMechanic.Services
{
    public class ProfileManager : IProfileManager
    {
        private const string AppFolderName = "TDX-AirMechanic";
        private const string DefaultProfileName = "default";
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public string GetProfilesFolderPath()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = Path.Combine(basePath, AppFolderName);
            Directory.CreateDirectory(path);
            return path;
        }

        public IReadOnlyList<string> ListProfiles()
        {
            var folder = GetProfilesFolderPath();
            if (!Directory.Exists(folder)) return Array.Empty<string>();
            var files = Directory.GetFiles(folder, "*.json");

            var names = new List<string>();
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("Model", out var modelProp))
                    {
                        var model = modelProp.GetString();
                        if (!string.IsNullOrWhiteSpace(model))
                        {
                            names.Add(model!);
                            continue;
                        }
                    }
                }
                catch
                {
                    // ignore and fall back to filename
                }
                names.Add(Path.GetFileNameWithoutExtension(file));
            }

            return names.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n).ToList();
        }

        public AirplaneProfile LoadProfileForModel(string? modelName)
        {
            // Determine filename
            var safeName = string.IsNullOrWhiteSpace(modelName) ? DefaultProfileName : SanitizeFileName(modelName!);
            var folder = GetProfilesFolderPath();
            var filePath = Path.Combine(folder, safeName + ".json");

            // Try model profile, else default, else create default
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var profile = JsonSerializer.Deserialize<AirplaneProfile>(json);
                    if (profile != null)
                    {
                        // Ensure model set
                        profile.Model = modelName ?? DefaultProfileName;
                        return profile;
                    }
                }
                catch { /* swallow and fallback */ }
            }

            // Try default if not already default
            if (safeName != DefaultProfileName)
            {
                var defaultPath = Path.Combine(folder, DefaultProfileName + ".json");
                if (File.Exists(defaultPath))
                {
                    try
                    {
                        var json = File.ReadAllText(defaultPath);
                        var profile = JsonSerializer.Deserialize<AirplaneProfile>(json) ?? new AirplaneProfile();
                        profile.Model = modelName ?? DefaultProfileName;
                        return profile;
                    }
                    catch { /* swallow */ }
                }
            }

            // Create an initial default profile
            var newProfile = new AirplaneProfile
            {
                Model = modelName ?? DefaultProfileName,
                CenteredSpring = true,
                DynamicSpring = false,
                StickShaker = true
            };

            // Ensure default exists on disk for future
            try
            {
                var defaultPath = Path.Combine(folder, DefaultProfileName + ".json");
                if (!File.Exists(defaultPath))
                {
                    var json = JsonSerializer.Serialize(new AirplaneProfile
                    {
                        Model = DefaultProfileName,
                        CenteredSpring = true,
                        DynamicSpring = false,
                        StickShaker = true
                    }, JsonOptions);
                    File.WriteAllText(defaultPath, json);
                }
            }
            catch { /* ignore */ }

            return newProfile;
        }

        public void SaveProfile(AirplaneProfile profile)
        {
            if (profile == null) return;
            var name = string.IsNullOrWhiteSpace(profile.Model) ? DefaultProfileName : SanitizeFileName(profile.Model);
            var folder = GetProfilesFolderPath();
            var path = Path.Combine(folder, name + ".json");
            try
            {
                var json = JsonSerializer.Serialize(profile, JsonOptions);
                File.WriteAllText(path, json);
            }
            catch
            {
                // swallow: UI should handle errors via calls returning progress, but spec asks for auto-save
            }
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var pattern = "[" + invalid + "]";
            var sanitized = Regex.Replace(name, pattern, "_");
            // Trim and collapse whitespace to single spaces
            sanitized = Regex.Replace(sanitized.Trim(), "\\s+", " ");
            return string.IsNullOrWhiteSpace(sanitized) ? DefaultProfileName : sanitized;
        }
    }
}
