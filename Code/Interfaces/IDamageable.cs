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
        public void TakeDamage(DamageData damageData);

        /// <summary>
        /// Handles what happens when a damageable dies.
        /// </summary>
        public void HandleDeath();
    }
}
