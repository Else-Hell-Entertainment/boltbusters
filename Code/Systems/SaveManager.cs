// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using Godot;
using Godot.Collections;

namespace EHE.BoltBusters.Systems
{
    /// <summary>
    ///  Manages serialization and deserialization of game save data to and
    ///  from JSON files. Provides methods for writing <see cref="Dictionary"/>
    ///  objects to files and reading them back.
    /// </summary>
    public class SaveManager
    {
        private const string ERR_FAILED_TO_OPEN_FILE = "Failed to open file at path '{0}'.";

        /// <summary>
        ///  Writes save data to a file in JSON format.
        /// </summary>
        ///
        /// <param name="path">
        ///  The file path where the save data will be written.
        /// </param>
        /// <param name="saveData">
        ///  The Dictionary containing the save data to serialize.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if the write operation was successful;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool WriteToFile(string path, Dictionary saveData)
        {
            var dataString = Json.Stringify(saveData);
            var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

            if (file == null)
            {
                GD.PushError(string.Format(ERR_FAILED_TO_OPEN_FILE, path));
                return false;
            }

            file.StoreString(dataString);
            file.Close();
            return true;
        }

        /// <summary>
        ///  Reads save data from a JSON file and deserializes it into a
        ///  <see cref="Dictionary"/>.
        /// </summary>
        ///
        /// <param name="path">
        ///  The file path to read the save data from.
        /// </param>
        ///
        /// <returns>
        ///  A <see cref="Dictionary"/> containing the deserialized save data
        ///  if the data was read and parsed successfully;
        ///  <c>null</c> otherwise.
        /// </returns>
        public Dictionary ReadFromFile(string path)
        {
            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

            if (file == null)
            {
                GD.PushError(string.Format(ERR_FAILED_TO_OPEN_FILE, path));
                return null;
            }

            var dataString = file.GetAsText();
            var dataObject = Json.ParseString(dataString);
            var dataObjectType = dataObject.VariantType;

            if (dataObjectType != Variant.Type.Dictionary)
            {
                GD.PushError($"Cannot parse save data: invalid format '{dataObjectType}'.");
                return null;
            }

            file.Close();
            return (Dictionary)dataObject;
        }
    }
}
