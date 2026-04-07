// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.Common.Godot;
using Godot;
using Godot.Collections;

namespace EHE.BoltBusters.Data
{
    /// <summary>
    ///  Generic base class for settings resources that can load and store their
    ///  values from/to a <see cref="Godot.Collections.Dictionary"/>.
    /// </summary>
    ///
    /// <typeparam name="T">
    ///  Type of the settings resource class, must inherit
    ///  <see cref="Resource"/>
    /// </typeparam>
    public abstract partial class SettingsResource<T> : Resource, ISerializable<T>
        where T : Resource, new()
    {
        /// <summary>
        ///  <para>
        ///   Loads the values from <paramref name="data"/> and stores them
        ///   into the instance. If <paramref name="defaults"/> is provided,
        ///   any missing values in <paramref name="data"/> will be filled in
        ///   from that object.
        ///  </para>
        ///  <para>
        ///   Note! This method does not apply the loaded values. This must
        ///   be done separately using the <see cref="ApplyValues"/>
        ///   method.
        ///  </para>
        /// </summary>
        ///
        /// <param name="data">Dictionary containing the data</param>
        /// <param name="defaults"></param>
        public abstract void Load(Dictionary data, T defaults = null);

        /// <summary>
        ///  Stores the values from runtime memory into this instance.
        /// </summary>
        public abstract void StoreValues();

        /// <summary>
        ///  Applies the values from this instance into runtime memory.
        /// </summary>
        public abstract void ApplyValues();

        /// <summary>
        ///  Resets all values in this instance to their default values.
        /// </summary>
        public abstract void ResetValues();

        /// <summary>
        ///  Deserialized a <see cref="Godot.Collections.Dictionary"/> into an
        ///  instance of type T where T must be of type
        ///  <see cref="SettingsResource{T}"/>.
        /// </summary>
        ///
        /// <param name="data">
        ///  Dictionary that represents the data of <c>T</c>.
        /// </param>
        ///
        /// <returns>
        ///  An instance of <c>T</c> if deserialization is successful,
        ///  otherwise <c>null</c>.
        /// </returns>
        public static T Deserialize(Dictionary data)
        {
            T obj = new T();

            // Written through GitHub Copilot auto-complete assistance.
            if (obj is SettingsResource<T> settingsResource)
            {
                settingsResource.Load(data, null);
                return obj;
            }

            GD.PrintErr("Cannot serialize object that is not a SettingsResource.");
            return null;
        }

        /// <summary>
        ///  Returns the values stored in this instance as a
        ///  <see cref="Godot.Collections.Dictionary"/>.
        /// </summary>
        public abstract Dictionary Serialize();
    }
}
