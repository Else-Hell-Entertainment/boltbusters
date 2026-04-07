// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.Data;
using Godot;

namespace EHE.BoltBusters.Config
{
    /// <summary>
    ///  Config module required by <see cref="VideoSettingsData"/>.
    /// </summary>
    public static class VideoSettingsConfig
    {
        /// <summary>
        ///  The key used to store video settings in the settings JSON file.
        /// </summary>
        public static readonly StringName SettingsFileSectionName = "Video";

        /// <summary>
        ///  Key for the resolution multiplier setting in the settings JSON
        ///  file.
        /// </summary>
        public static readonly StringName KeyResolutionMultiplier = "ResolutionMultiplier";

        /// <summary>
        ///  Key for the fullscreen setting in the settings JSON file.
        /// </summary>
        public static readonly StringName KeyIsFullscreen = "IsFullscreen";

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
