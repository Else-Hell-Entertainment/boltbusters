// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System;

namespace EHE.BoltBusters.Config
{
    public static class FilePathConfig
    {
        #region Level Scenes

        /// <summary>
        /// Path to the background level scene file.
        /// </summary>
        /// <seealso cref="LevelManager"/>
        public const string BACKGROUND_LEVEL_SCENE_PATH = "res://Scenes/Level/BackgroundLevel.tscn";

        /// <summary>
        /// Path to the gameplay level scene file.
        /// </summary>
        /// <seealso cref="LevelManager"/>
        public const string GAMEPLAY_LEVEL_SCENE_PATH = "res://Scenes/Level/Level.tscn";

        #endregion Level Scenes


        #region Camera

        /// <summary>
        /// Path to the <see cref="CameraRig"/> scene file.
        /// </summary>
        [Obsolete]
        public const string CAMERA_SCENE_PATH = "res://Scenes/CameraRig.tscn";

        /// <summary>
        /// Path to the camera shader material file.
        /// </summary>
        [Obsolete]
        public const string CAMERA_SHADER_MATERIAL_PATH = "res://Assets/Materials/PS1CameraShaderMaterial.tres";

        /// <summary>
        /// Path to the <see cref="CameraContainer"/> scene file.
        /// </summary>
        public const string CAMERA_CONTAINER_SCENE_PATH = "res://Scenes/Camera/CameraContainer.tscn";

        #endregion Camera


        #region Round Data

        /// <summary>
        /// Path to the round data resource file. When formatting the string,
        /// {0} is replaced with the round index.
        /// </summary>
        public const string ROUND_DATA_FILE_PATH_FORMAT = "res://Data/Round/RoundData{0}.tres";

        #endregion Round Data


        #region Player Data

        /// <summary>
        ///  The path to the default player data resource file.
        /// </summary>
        public const string DEFAULT_PLAYER_DATA_RESOURCE_PATH = "res://Data/Player/DefaultPlayerData.tres";

        #endregion Player Data
    }
}
