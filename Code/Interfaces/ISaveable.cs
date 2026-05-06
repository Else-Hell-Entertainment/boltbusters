// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using Godot.Collections;

namespace EHE.Common.Godot
{
    /// <summary>
    ///  Provides an interface that lets the save system of the game to easily
    ///  walk through objects in the scene tree and save them to a save file.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        ///  Saves the object to a Godot <see cref="Dictionary"/>.
        /// </summary>
        ///
        /// <returns>
        ///  A Godot Dictionary representing the data of the object.
        /// </returns>
        Dictionary Save();

        /// <summary>
        ///  Loads data to the object from a Godot <see cref="Dictionary"/>.
        /// </summary>
        ///
        /// <param name="data">
        ///  A Godot Dictionary representing the data of the object.
        /// </param>
        void Load(Dictionary data);
    }
}
