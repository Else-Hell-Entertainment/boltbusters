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
            public const string SETTINGS_FILE_SECTION_NAME = "Audio";

            /// <summary>
            ///  Name of the master audio bus.
            /// </summary>
            public const string MASTER_BUS_NAME = "Master";

            /// <summary>
            ///  Name of the music audio bus.
            /// </summary>
            public const string MUSIC_BUS_NAME = "Music";

            /// <summary>
            ///  Name of the sound effects bus.
            /// </summary>
            public const string SFX_BUS_NAME = "Sfx";

            /// <summary>
            ///  Default master volume on a linear scale,
            ///  0 = silent, 1 = max volume.
            /// </summary>
            public const float DEFAULT_MASTER_VOLUME = 1.0f;

            /// <summary>
            ///  Default music volume on a linear scale,
            ///  0 = silent, 1 = max volume.
            /// </summary>
            public const float DEFAULT_MUSIC_VOLUME = 1.0f;

            /// <summary>
            ///  Default sound effects volume on a linear scale,
            ///  0 = silent, 1 = max volume.
            /// </summary>
            public const float DEFAULT_SFX_VOLUME = 1.0f;
        }

        /// <summary>
        ///  Config module required by <see cref="VideoSettingsData"/>.
        /// </summary>
        public static class Video
        {
            /// <summary>
            ///  The key used to store video settings in the settings JSON file.
            /// </summary>
            public const string SETTINGS_FILE_SECTION_NAME = "Video";

            /// <summary>
            ///  Key for the resolution multiplier setting in the settings JSON
            ///  file.
            /// </summary>
            public const string KEY_RESOLUTION_MULTIPLIER = "ResolutionMultiplier";

            /// <summary>
            ///  Key for the fullscreen setting in the settings JSON file.
            /// </summary>
            public const string KEY_IS_FULLSCREEN = "IsFullscreen";

            /// <summary>
            ///  Base resolution of the game. Should be the same as defined in
            ///  the project settings.
            /// </summary>
            public static readonly Vector2I BaseResolution = new(640, 360);

            /// <summary>
            ///  Default resolution multiplier. Recommended value is 1.
            /// </summary>
            public const int DEFAULT_RESOLUTION_MULTIPLIER = 1;

            /// <summary>
            ///  Default state of fullscreen toggle. Typically false.
            /// </summary>
            public const bool DEFAULT_IS_FULLSCREEN = false;
        }
    }
}
