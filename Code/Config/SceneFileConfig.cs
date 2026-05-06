// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System;
using Godot;

namespace EHE.BoltBusters.Config
{
    /// <summary>
    /// <b>Deprecated</b>, use <see cref="FilePathConfig"/> instead.
    /// Paths to scene files.
    /// </summary>
    [Obsolete("Use FilePathConfig")]
    public partial class SceneFileConfig : Node
    {
        // Levels
        [Obsolete("Use FilePathConfig.BACKGROUND_LEVEL_SCENE_PATH")]
        public static string BACKGROUND_LEVEL_PATH => FilePathConfig.BACKGROUND_LEVEL_SCENE_PATH;

        [Obsolete("Use FilePathConfig.GAMEPLAY_LEVEL_SCENE_PATH")]
        public static string GAMEPLAY_LEVEL_PATH => FilePathConfig.GAMEPLAY_LEVEL_SCENE_PATH;

        // Camera
        [Obsolete("Use FilePathConfig.CAMERA_SCENE_PATH")]
        public static string CAMERA_FILE => FilePathConfig.CAMERA_SCENE_PATH;
    }
}
