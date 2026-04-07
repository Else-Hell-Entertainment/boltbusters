// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.Data;
using Godot;
using Godot.Collections;

namespace EHE.BoltBusters
{
    /// <summary>
    ///  This class is responsible for saving and loading settings.
    /// </summary>
    public class SettingsManager
    {
        /// <summary>
        ///  Instance of SettingsData containing the default values for the
        ///  settings.
        /// </summary>
        public readonly SettingsData DefaultSettingsData;

        /// <summary>
        ///  Settings data that is currently store in memory. Note! This data is
        ///  not necessarily the same as what is stored on disk!
        /// </summary>
        public SettingsData CurrentSettingsData { get; private set; }

        public SettingsManager() { }

        /// <summary>
        ///  Creates a new SettingsManager with the specified default settings.
        /// </summary>
        ///
        /// <param name="defaults"></param>
        public SettingsManager(SettingsData defaults)
        {
            DefaultSettingsData = defaults;
        }

        /// <summary>
        ///  Reads the settings from a JSON file.
        /// </summary>
        ///
        /// <param name="filePath">Path to the JSON file to read.</param>
        public SettingsData LoadSettingsFromFile(string filePath)
        {
            var settingsFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);

            if (settingsFile == null)
            {
                GD.Print($"Failed to read settings from file '{filePath}'. Using default settings.");
                return DefaultSettingsData;
            }

            var jsonString = settingsFile.GetLine();
            var json = new Json();
            var parseError = json.Parse(jsonString);

            if (parseError != Error.Ok)
            {
                GD.Print(
                    $"Failed to parse settings: "
                        + $"{json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}. "
                        + $"Using default settings."
                );
                return DefaultSettingsData;
            }

            var settingsDict = (Dictionary)json.Data;

            settingsFile.Close();
            settingsFile.Dispose();

#if DEBUG
            GD.Print($"Read the following settings from file '{filePath}':");
            GD.Print(jsonString);
#endif

            return SettingsData.Deserialize(settingsDict);
        }

        /// <summary>
        ///  Writes the current settings to a JSON file.
        /// </summary>
        ///
        /// <param name="filePath">Path to the JSON file to write.</param>
        ///
        /// <returns>
        /// </returns>
        public bool SaveSettingsToFile(string filePath)
        {
            var settingsFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);

            if (settingsFile == null)
            {
                GD.PrintErr($"Failed to write settings to file '{filePath}'.");
                return false;
            }

            var jsonString = Json.Stringify(CurrentSettingsData.Serialize());
            settingsFile.StoreLine(jsonString);
            settingsFile.Close();
            settingsFile.Dispose();

#if DEBUG
            GD.Print($"Wrote the following settings to file '{filePath}':");
            GD.Print(jsonString);
#endif

            return true;
        }

        /// <summary>
        ///  Fetches the current settings from memory and saves them to the
        ///  <see cref="CurrentSettingsData"/> instance.
        /// </summary>
        public void SaveSettings()
        {
            CurrentSettingsData.StoreValues();
        }

        /// <summary>
        ///  Applies settings using the values from the provided
        ///  <see cref="SettingsData"/> instance.
        /// </summary>
        ///
        /// <param name="data"></param>
        public void ApplySettings(SettingsData data)
        {
            GD.Print("SettingsManager - Applying settings from data dictionary.");
            CurrentSettingsData = data;
            CurrentSettingsData.ApplyValues();
        }

        /// <summary>
        ///  Resets the current settings to the default values.
        /// </summary>
        public void ResetSettings()
        {
            CurrentSettingsData = (SettingsData)DefaultSettingsData.Duplicate(deep: true);
        }
    }
}
