// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System;
using EHE.BoltBusters.Config;
using EHE.BoltBusters.Data;
using EHE.Common.Godot.Logging;
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
        public SettingsData DefaultSettingsData { get; private set; }

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
        [Obsolete("Use `new SettingsManager()` and `Initialize()` instead.")]
        public SettingsManager(SettingsData defaults)
        {
            DefaultSettingsData = defaults;
        }

        /// <summary>
        ///  Initializes the SettingsManager by loading default settings.
        /// </summary>
        ///
        /// <param name="defaultPathOverride">
        ///  Path to the resource file to use for default settings. Default is
        ///  <c>null</c>.
        /// </param>
        ///
        /// <remarks>
        ///  If a value for <paramref name="defaultPathOverride"/> is provided,
        ///  loads the default setting from the resource file located at that
        ///  path. If no value is provided, uses the default path defined by
        ///  <see cref="SettingsConfig.DEFAULT_SETTINGS_FILE_PATH"/>.
        /// </remarks>
        public void Initialize(string defaultPathOverride = null)
        {
            LoadDefaultsFromFile(defaultPathOverride ?? SettingsConfig.DEFAULT_SETTINGS_FILE_PATH);
            ApplySettings(DefaultSettingsData);
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
                this.LogWarning($"Failed to read settings from file '{filePath}'. Using default settings.");
                return DefaultSettingsData;
            }

            var jsonString = settingsFile.GetLine();
            var json = new Json();
            var parseError = json.Parse(jsonString);

            if (parseError != Error.Ok)
            {
                this.LogError(
                    $"Failed to parse settings: "
                        + $"{json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}. "
                        + $"Using default settings."
                );
                return DefaultSettingsData;
            }

            var settingsDict = (Dictionary)json.Data;

            settingsFile.Close();
            settingsFile.Dispose();

            this.LogDebug($"Read the following settings from file '{filePath}':");
            this.LogDebug(jsonString);

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
                this.LogError($"Failed to write settings to file '{filePath}'.");
                return false;
            }

            var jsonString = Json.Stringify(CurrentSettingsData.Serialize());
            settingsFile.StoreLine(jsonString);
            settingsFile.Close();
            settingsFile.Dispose();

            this.LogDebug($"Wrote the following settings to file '{filePath}':");
            this.LogDebug(jsonString);

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
            this.LogInfo("Applying settings from data dictionary.");
            CurrentSettingsData = data;
            CurrentSettingsData.ApplyValues();
        }

        /// <summary>
        ///  Resets the current settings to the default values.
        /// </summary>
        public void ResetSettings()
        {
            CurrentSettingsData = (SettingsData)DefaultSettingsData.Duplicate(true);
        }

        /// <summary>
        ///  Loads the default settings into memory from the given path.
        /// </summary>
        ///
        /// <param name="filePath">
        ///  Path to the default settings resource file.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if default settings were loaded successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        private bool LoadDefaultsFromFile(string filePath)
        {
            DefaultSettingsData = GD.Load<SettingsData>(filePath);

            if (DefaultSettingsData == null)
            {
                this.LogError($"Failed to load default settings from path '{filePath}'!");
                return false;
            }

            return true;
        }
    }
}
