// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>
//
// Modified from original implementation by Sami Kojo under 3-Clause BSD License.
// Source: https://github.com/samikojo-tuni/GArkanoid-2025/blob/8ce89d9e5dea3cdaa829bfdf62549168046435db/Code/UI/UIOptions.cs
// License: https://github.com/samikojo-tuni/GArkanoid-2025/blob/8ce89d9e5dea3cdaa829bfdf62549168046435db/LICENSE

using EHE.BoltBusters.Config;
using Godot;

namespace EHE.BoltBusters.Ui
{
    /// <summary>
    ///  Controls the behavior of the audio tab in the settings menu.
    /// </summary>
    public partial class AudioTab : Control
    {
        [Export]
        private VolumeSlider _masterVolumeSlider;

        [Export]
        private VolumeSlider _musicVolumeSlider;

        [Export]
        private VolumeSlider _sfxVolumeSlider;

        [Export]
        private string _masterDisplayName;

        [Export]
        private string _musicDisplayName;

        [Export]
        private string _sfxDisplayName;

        public void Initialize()
        {
            SetupVolumeSlider(_masterVolumeSlider, AudioSettingsConfig.MasterBusName, _masterDisplayName);
            SetupVolumeSlider(_musicVolumeSlider, AudioSettingsConfig.MusicBusName, _musicDisplayName);
            SetupVolumeSlider(_sfxVolumeSlider, AudioSettingsConfig.SfxBusName, _sfxDisplayName);
        }

        public override void _EnterTree()
        {
            _masterVolumeSlider.VolumeChanged += OnVolumeChanged;
            _musicVolumeSlider.VolumeChanged += OnVolumeChanged;
            _sfxVolumeSlider.VolumeChanged += OnVolumeChanged;
        }

        public override void _ExitTree()
        {
            _masterVolumeSlider.VolumeChanged -= OnVolumeChanged;
            _musicVolumeSlider.VolumeChanged -= OnVolumeChanged;
            _sfxVolumeSlider.VolumeChanged -= OnVolumeChanged;
        }

        /// <summary>
        ///  Sets up a volume slider for a specific audio bus.
        /// </summary>
        ///
        /// <param name="volumeSlider">
        ///  volume slider instance
        /// </param>
        /// <param name="busName">
        ///  name of the associated audio bus
        /// </param>
        /// <param name="displayName">
        ///  text displayed in the settings menu
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if the setup was successful,
        ///  <c>false</c> otherwise.
        /// </returns>
        private bool SetupVolumeSlider(VolumeSlider volumeSlider, string busName, string displayName)
        {
            var busIndex = AudioServer.GetBusIndex(busName);
            var validBus = busIndex >= 0;

            if (validBus)
            {
                var volumeDb = AudioServer.GetBusVolumeDb(busIndex);
                volumeSlider.Setup(busName, displayName, volumeDb);
            }
            else
            {
                GD.PrintErr($"Failed to initialize volume slider, invalid bus name '{busName}'");
            }

            return validBus;
        }

        /// <summary>
        ///  Called when the volume of a bus is changed via the volume slider.
        /// </summary>
        ///
        /// <param name="busName">
        ///  name of the audio bus whose volume should change
        /// </param>
        /// <param name="volumeDb">
        ///  new value for the volume in decibels
        /// </param>
        private void OnVolumeChanged(string busName, float volumeDb)
        {
            var busIndex = AudioServer.GetBusIndex(busName);
            if (busIndex >= 0)
            {
                AudioServer.SetBusVolumeDb(busIndex, volumeDb);
            }
        }
    }
}
