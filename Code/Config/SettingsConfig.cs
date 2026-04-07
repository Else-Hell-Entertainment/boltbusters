// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.Data;
using Godot;

namespace EHE.BoltBusters.Config
{
    public static class SettingsConfig
    {
        /// <summary>
        ///  Config module required by <see cref="AudioSettingsData"/>.
        /// </summary>
        public static class Audio
        {
            /// <summary>
            ///  The key used to store audio settings in the settings JSON file.
            /// </summary>
            public static readonly string SettingsFileSectionName = "Audio";

            /// <summary>
            ///  Name of the master audio bus.
            /// </summary>
            public static readonly string MasterBusName = "Master";

            /// <summary>
            ///  Name of the music audio bus.
            /// </summary>
            public static readonly string MusicBusName = "Music";

            /// <summary>
            ///  Name of the sound effects bus.
            /// </summary>
            public static readonly string SfxBusName = "Sfx";

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

        /// <summary>
        ///  Config module required by <see cref="VideoSettingsData"/>.
        /// </summary>
        public static class Video
        {
            /// <summary>
            ///  The key used to store video settings in the settings JSON file.
            /// </summary>
            public static readonly string SettingsFileSectionName = "Video";

            /// <summary>
            ///  Key for the resolution multiplier setting in the settings JSON
            ///  file.
            /// </summary>
            public static readonly string KeyResolutionMultiplier = "ResolutionMultiplier";

            /// <summary>
            ///  Key for the fullscreen setting in the settings JSON file.
            /// </summary>
            public static readonly string KeyIsFullscreen = "IsFullscreen";

            /// <summary>
            ///  Base resolution of the game. Should be the same as defined in
            ///  the project settings.
            /// </summary>
            public static readonly Vector2I BaseResolution = new(640, 360);

            /// <summary>
            ///  Default resolution multiplier. Recommended value is 1.
            /// </summary>
            public const int DefaultResolutionMultiplier = 1;

            /// <summary>
            ///  Default state of fullscreen toggle. Typically false.
            /// </summary>
            public const bool DefaultIsFullscreen = false;
        }
    }
}
