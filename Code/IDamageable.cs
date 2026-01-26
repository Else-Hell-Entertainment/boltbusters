using System;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Interface for objects that can take damage and have health.
    /// </summary>
    public interface IDamageable
    {
        // TODO: Docs.
        public void Heal(int amount);

        // TODO: Docs.
        public void TakeDamage(Damage damageObj);

        /// <summary>
        /// Handles what happens when a damageable dies. By default, this only
        /// prints a debug message to console.
        /// </summary>
        public virtual void HandleDeath()
        {
            GD.Print("I died.");
        }
    }
}
