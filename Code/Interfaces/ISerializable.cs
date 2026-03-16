// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using Godot;
using Godot.Collections;

namespace EHE.Common.Godot
{
    /// <summary>
    ///  Provides an interface for serializing and deserializing
    ///  <see cref="GodotObject"/>s to and from <see cref="Dictionary"/>
    ///  instances.
    /// </summary>
    ///
    /// <typeparam name="T">
    ///  Type of the serializable class; must inherit the
    ///  <see cref="GodotObject"/> class.
    /// </typeparam>
    public interface ISerializable<T>
        where T : GodotObject
    {
        /// <summary>
        ///  Deserializes a <see cref="Dictionary"/> into an
        ///  instance of type T.
        /// </summary>
        ///
        /// <param name="data">
        ///  Data to load into the object.
        /// </param>
        ///
        /// <returns>
        ///  An instance of T that has been set up with the given data.
        /// </returns>
        ///
        /// <remarks>
        ///  T is derived from the <see cref="GodotObject"/> class.
        /// </remarks>
        public static abstract T Deserialize(Dictionary data);

        /// <summary>
        ///  Serializes an instance of type T into a
        ///  <see cref="Dictionary"/>.
        /// </summary>
        ///
        /// <remarks>
        ///  T must be derived from the <see cref="GodotObject"/> class.
        /// </remarks>
        public Dictionary Serialize();
    }
}
