// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano tuominen.mika-95@hotmail.com

using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Base class for all collectible objects in the game.
    /// Handles general spawn, collection, and despawn behavior,
    /// and provides a common initialization path for specific collectible types.
    /// </summary>
    public partial class Collectible : Area3D, ISpawnable, ICollectible
    {
        private CollectibleType _collectibleType = CollectibleType.None;

        public CollectibleType CollectibleType
        {
            get { return _collectibleType; }
            protected set { _collectibleType = value; }
        }

        /// <summary>
        /// Initializes the collectible with a specific type.
        /// Called after the object is created but before it is spawned.
        /// </summary>
        /// <param name="collectibleType">The type assigned to this collectible instance.</param>
        public void Initialize(CollectibleType collectibleType)
        {
            CollectibleType = collectibleType;
        }

        /// <summary>
        /// Called when the collectible is spawned into the scene.
        /// Override this method in derived classes to add visuals, audio,
        /// or any setup logic needed before the collectible becomes active.
        /// </summary>
        public virtual void OnSpawn()
        {
            // TODO: Add general spawn behavior for all collectible types.
            // Examples:
            // - Play a generic spawn animation (hover, bounce, fade-in)
            // - Play a spawn sound effect
            // - Start a light bobbing or spinning animation
            // - Trigger a small particle effect (sparkle on appearance)
        }

        /// <summary>
        /// Called when the collectible is collected by a character.
        /// Override this in derived types to implement collection logic,
        /// such as granting currency, power-ups, or triggering effects.
        /// </summary>
        /// <param name="collector">The character that collected this item.</param>
        public virtual void OnCollect(CharacterBody3D collector)
        {
            // TODO: Add general collect behavior for all collectible types.
            // Examples:
            // - Apply currency or buff logic through the collector
            // - Play pickup sound
            // - Show a pickup VFX at the collectible position
            // - Show a small UI popup ("+1", "+5", etc.)
            // - Trigger camera punch, screen shake, or other feedback effects
        }

        /// <summary>
        /// Called when the collectible is removed from the scene.
        /// Override this to add visual effects, sound cues,
        /// or cleanup behavior before the object is destroyed or returned to a pool.
        /// </summary>
        public virtual void OnDespawn()
        {
            // TODO: Add general despawn behavior for all collectible types.
            // Examples:
            // - Play a fade-out animation
            // - Trigger a disappearing particle effect
            // - Delay the final QueueFree() slightly if needed for VFX

            QueueFree();
        }
    }
}
