// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

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
