// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.Config;
using Godot;
using Godot.Collections;

namespace EHE.BoltBusters.Data
{
    /// <summary>
    ///  Settings resource that holds other settings resources.
    /// </summary>
    [GlobalClass]
    public sealed partial class SettingsData : SettingsResource<SettingsData>
    {
        /// <summary>
        ///  Holds the audio settings data.
        /// </summary>
        [Export]
        public AudioSettingsData AudioSettingsData = new AudioSettingsData();

        /// <summary>
        ///  Holds the video settings data.
        /// </summary>
        [Export]
        public VideoSettingsData VideoSettingsData = new VideoSettingsData();

        public override void Load(Dictionary data, SettingsData defaults = null)
        {
            GD.Print("[SettingsData] Loading from data dictionary.");

            if (defaults == null)
            {
                GD.Print("[SettingsData] No defaults provided, using new instance as fallback.");
                defaults = new SettingsData();
            }

            var audioDict = (Dictionary)data[AudioSettingsConfig.SettingsFileSectionName];
            var videoDict = (Dictionary)data[VideoSettingsConfig.SettingsFileSectionName];

            AudioSettingsData = AudioSettingsData.Deserialize(audioDict);
            VideoSettingsData = VideoSettingsData.Deserialize(videoDict);

            AudioSettingsData ??= defaults.AudioSettingsData;
            VideoSettingsData ??= defaults.VideoSettingsData;
        }

        public override void StoreValues()
        {
            AudioSettingsData.StoreValues();
            VideoSettingsData.StoreValues();
        }

        public override void ApplyValues()
        {
            AudioSettingsData.ApplyValues();
            VideoSettingsData.ApplyValues();
        }

        public override void ResetValues()
        {
            AudioSettingsData.ResetValues();
            VideoSettingsData.ResetValues();
        }

        public override Dictionary Serialize()
        {
            var dict = new Dictionary();
            dict.Add(AudioSettingsConfig.SettingsFileSectionName, AudioSettingsData.Serialize());
            dict.Add(VideoSettingsConfig.SettingsFileSectionName, VideoSettingsData.Serialize());
            return dict;
        }
    }
}
