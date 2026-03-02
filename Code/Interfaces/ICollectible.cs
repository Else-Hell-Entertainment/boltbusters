// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano tuominen.mika-95@hotmail.com

using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Represents an object that can be collected by a character.
    /// Implementations define what happens when the object is picked up.
    /// </summary>
    public interface ICollectible
    {
        /// <summary>
        /// Called when the collectible is picked up by a character.
        /// Use this to apply effects, rewards, or any gameplay logic triggered by the collection event.
        /// </summary>
        /// <param name="collector">The character that collected the item.</param>
        void OnCollect(CharacterBody3D collector);
    }
}
