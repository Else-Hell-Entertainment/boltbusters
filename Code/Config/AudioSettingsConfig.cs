// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.Data;
using Godot;

namespace EHE.BoltBusters.Config
{
    /// <summary>
    ///  Config module required by <see cref="AudioSettingsData"/>.
    /// </summary>
    public static class AudioSettingsConfig
    {
        /// <summary>
        ///  The key used to store audio settings in the settings JSON file.
        /// </summary>
        public static readonly StringName SettingsFileSectionName = "Audio";

        /// <summary>
        ///  Name of the master audio bus.
        /// </summary>
        public static readonly StringName MasterBusName = "Master";

        /// <summary>
        ///  Name of the music audio bus.
        /// </summary>
        public static readonly StringName MusicBusName = "Music";

        /// <summary>
        ///  Name of the sound effects bus.
        /// </summary>
        public static readonly StringName SfxBusName = "Sfx";

        /// <summary>
        ///  Default master volume on a linear scale,
        ///  0 = silent, 1 = max volume.
        /// </summary>
        public const float DefaultMasterVolume = 1.0f;

        /// <summary>
        ///  Default music volume on a linear scale,
        ///  0 = silent, 1 = max volume.
        /// </summary>
        public const float DefaultMusicVolume = 1.0f;

        /// <summary>
        ///  Default sound effects volume on a linear scale,
        ///  0 = silent, 1 = max volume.
        /// </summary>
        public const float DefaultSfxVolume = 1.0f;
    }
}
