// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano tuominen.mika-95@hotmail.com

namespace EHE.BoltBusters
{
    /// <summary>
    /// Defines common spawn and despawn lifecycle events for objects
    /// that appear in the game world.
    /// </summary>
    public interface ISpawnable
    {
        /// <summary>
        /// Called immediately after the object has spawned.
        /// Used for initializing state, starting animations,
        /// or performing setup that must happen post‑spawn.
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// Called when the object is requested to despawn.
        /// Used for playing despawn animations
        /// (for characters, see <see cref="Character.HandleDeath()"/> before this method is invoked),
        /// cleaning up state, or queueing the object for removal / returning the object to a pooling system.
        /// </summary>
        void OnDespawn();
    }
}
