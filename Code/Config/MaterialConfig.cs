// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System;

namespace EHE.BoltBusters.Config
{
    /// <summary>
    /// <b>Deprecated</b>, use <see cref="FilePathConfig"/> instead.
    /// </summary>
    [Obsolete("Use FilePathConfig")]
    public static class MaterialConfig
    {
        [Obsolete("Use FilePathConfig.CAMERA_SHADER_MATERIAL_PATH")]
        public static string CAMERA_SHADER_MATERIAL_FILE => FilePathConfig.CAMERA_SHADER_MATERIAL_PATH;
    }
}
