// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.Config;
using EHE.Common.Godot;
using Godot;
using Godot.Collections;

namespace EHE.BoltBusters.Data
{
    /// <summary>
    ///  Settings resource that holds audio-related settings such as
    ///  volume levels for different audio buses. Requires the
    ///  <see cref="AudioSettingsConfig"/> config module.
    /// </summary>
    [GlobalClass]
    public sealed partial class AudioSettingsData
        : SettingsResource<AudioSettingsData>,
            ISerializable<AudioSettingsData>
    {
        [Export]
        private float _masterVolume = AudioSettingsConfig.DefaultMasterVolume;

        [Export]
        private float _musicVolume = AudioSettingsConfig.DefaultMusicVolume;

        [Export]
        private float _sfxVolume = AudioSettingsConfig.DefaultSfxVolume;

        /// <summary>
        ///  Linear volume of the master bus.
        ///  Ranges from 0.0 (silent) to 1.0 (full volume).
        /// </summary>
        public float MasterVolume
        {
            get => _masterVolume;
            set => RefSetVolume(ref _masterVolume, value);
        }

        /// <summary>
        ///  Linear volume of the music bus.
        ///  Ranges from 0.0 (silent) to 1.0 (full volume).
        /// </summary>
        public float MusicVolume
        {
            get => _musicVolume;
            set => RefSetVolume(ref _musicVolume, value);
        }

        /// <summary>
        ///  Linear volume of the sound effect bus.
        ///  Ranges from 0.0 (silent) to 1.0 (full volume).
        /// </summary>
        public float SfxVolume
        {
            get => _sfxVolume;
            set => RefSetVolume(ref _sfxVolume, value);
        }

        public override void Load(Dictionary data, AudioSettingsData defaults = null)
        {
            GD.Print("[AudioSettingsData] Loading from data dictionary.");

            if (defaults == null)
            {
                GD.Print("[AudioSettingsData] No defaults provided, using new instance as fallback.");
                defaults = new AudioSettingsData();
            }

            RefLoadVolume(ref _masterVolume, data, AudioSettingsConfig.MasterBusName, defaults.MasterVolume);
            RefLoadVolume(ref _musicVolume, data, AudioSettingsConfig.MusicBusName, defaults.MusicVolume);
            RefLoadVolume(ref _sfxVolume, data, AudioSettingsConfig.SfxBusName, defaults.SfxVolume);
        }

        /// <summary>
        ///  Reads the bus volumes from <see cref="AudioServer"/> and stores
        ///  them in this instance.
        /// </summary>
        public override void StoreValues()
        {
            RefStoreVolume(ref _masterVolume, AudioSettingsConfig.MasterBusName);
            RefStoreVolume(ref _musicVolume, AudioSettingsConfig.MusicBusName);
            RefStoreVolume(ref _sfxVolume, AudioSettingsConfig.SfxBusName);
        }

        /// <summary>
        ///  Applies the values stored in this instance to the
        ///  <see cref="AudioServer"/>.
        /// </summary>
        public override void ApplyValues()
        {
#if DEBUG
            GD.Print("[AudioSettingsData] Applying audio settings.");
            GD.Print("[AudioSettingsData] MasterVolume: " + MasterVolume);
            GD.Print("[AudioSettingsData] MusicVolume: " + MusicVolume);
            GD.Print("[AudioSettingsData] SfxVolume: " + SfxVolume);
#endif
            SetBusVolume(AudioSettingsConfig.MasterBusName, MasterVolume);
            SetBusVolume(AudioSettingsConfig.MusicBusName, MusicVolume);
            SetBusVolume(AudioSettingsConfig.SfxBusName, SfxVolume);
        }

        /// <summary>
        ///  Sets all volume settings to their default values as specified in
        ///  <see cref="AudioSettingsConfig"/> and applies them.
        /// </summary>
        public override void ResetValues()
        {
#if DEBUG
            GD.Print("[AudioSettingsData] Resetting audio settings to default values.");
#endif
            MasterVolume = AudioSettingsConfig.DefaultMasterVolume;
            MusicVolume = AudioSettingsConfig.DefaultMusicVolume;
            SfxVolume = AudioSettingsConfig.DefaultSfxVolume;
            ApplyValues();
        }

        /// <summary>
        ///  Returns the audio settings as a <see cref="Dictionary"/> where keys
        ///  are the bus names defined in <see cref="AudioSettingsConfig"/>
        ///  and values are the corresponding linear volume levels.
        /// </summary>
        public override Dictionary Serialize()
        {
            var dict = new Dictionary();
            dict.Add(AudioSettingsConfig.MasterBusName, MasterVolume);
            dict.Add(AudioSettingsConfig.MusicBusName, MusicVolume);
            dict.Add(AudioSettingsConfig.SfxBusName, SfxVolume);
            return dict;
        }

        /// <summary>
        ///  Sets the volume for the specified bus in the AudioServer.
        /// </summary>
        ///
        /// <param name="busName">Name of the audio bus.</param>
        /// <param name="linearVolume">Linear volume to be set.</param>
        ///
        /// <returns>
        ///  <c>true</c> if volume is set successfully,
        ///  <c>false</c>otherwise.
        /// </returns>
        private bool SetBusVolume(string busName, float linearVolume)
        {
            var busIndex = AudioServer.GetBusIndex(busName);
            var validBusIndex = busIndex >= 0;

            if (!validBusIndex)
            {
                GD.PushError($"Cannot set volume for invalid bus '{busName}'.");
                return false;
            }

            AudioServer.SetBusVolumeLinear(busIndex, linearVolume);
#if DEBUG
            GD.Print($"[AudioSettingsData] Set volume for bus '{busName}' to {linearVolume} (linear).");
            GD.Print(
                $"[AudioSettingsData] AudioServer reports volume as {AudioServer.GetBusVolumeLinear(busIndex)} "
                    + $"(linear)."
            );
#endif
            return true;
        }

        /// <summary>
        ///  Retrieves the linear volume for the specified bus from the
        ///  AudioServer.
        /// </summary>
        ///
        /// <param name="busName">
        ///  Name of the audio bus to read the volume from.
        /// </param>
        /// <param name="linearVolume">
        ///  Linear volume of the given audio bus. <see cref="float.NaN"/> if
        ///  volume cannot be retrieved.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if volume is retrieved successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        private bool GetBusVolume(string busName, out float linearVolume)
        {
            var busIndex = AudioServer.GetBusIndex(busName);
            linearVolume = float.NaN;

            if (busIndex < 0)
            {
                GD.PushError($"Cannot fetch volume for invalid bus '{busName}'.");
                return false;
            }

            linearVolume = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(busIndex));
            return true;
        }

        #region Ref methods

        /// <summary>
        ///  Sets the volume field by reference, clamping the value between
        ///  0.0 and 1.0.
        /// </summary>
        ///
        /// <param name="fieldRef">Reference to the volume field.</param>
        /// <param name="value">New value for the volume field.</param>
        private void RefSetVolume(ref float fieldRef, float value)
        {
            fieldRef = Mathf.Clamp(value, 0.0f, 1.0f);
        }

        /// <summary>
        ///  Loads the volume from the data dictionary into the specified
        ///  field by reference. If the key is not found, uses the provided
        ///  default value.
        /// </summary>
        ///
        /// <param name="fieldRef">Reference to the volume field.</param>
        /// <param name="data">Data dictionary containing the new value.</param>
        /// <param name="key">Dictionary key for the new value.</param>
        /// <param name="defaultValue">
        ///  Fallback value if reading the dictionary fails.
        /// </param>
        private void RefLoadVolume(ref float fieldRef, Dictionary data, string key, float defaultValue)
        {
            if (data.TryGetValue(key, out var volume))
            {
                RefSetVolume(ref fieldRef, (float)volume);
                return;
            }

            GD.PushError($"Failed to load {key} from data dictionary, using default value ({defaultValue}).");
            RefSetVolume(ref fieldRef, defaultValue);
        }

        /// <summary>
        ///  Stores the volume from the AudioServer into the specified
        ///  field by reference. If fetching the volume fails, keeps the
        ///  existing value.
        /// </summary>
        ///
        /// <param name="fieldRef">Name of the volume field.</param>
        /// <param name="busName">
        ///  Name of the audio bus from which the volume is fetched.
        /// </param>
        private void RefStoreVolume(ref float fieldRef, string busName)
        {
            if (GetBusVolume(busName, out var volume))
            {
                RefSetVolume(ref fieldRef, volume);
                return;
            }

            GD.PushError(
                $"Failed to store volume for '{busName}' from the AudioServer, keeping existing value ({fieldRef})."
            );
        }

        #endregion
    }
}
