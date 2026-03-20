// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

namespace EHE.BoltBusters
{
    /// <summary>
    /// Defines the type of the level. Used when switching
    /// <see cref="LevelManager"/>s during runtime.
    /// </summary>
    public enum LevelType
    {
        None = 0,
        Gameplay,
        Background,
    }
}
