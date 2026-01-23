using System;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Interface for objects that can take damage and have health.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Current health of the object. Override setter to implement health
        /// clamping.
        /// </summary>
        public abstract int CurrentHealth { get; set; }

        /// <summary>
        /// Increases health by the specified amount. The amount should be
        /// positive. If a negative amount is provided, it will be
        /// automatically converted to a positive value.
        /// </summary>
        /// <param name="amount">The amount to increase health by.</param>
        public virtual void Heal(int amount)
        {
            if (amount < 0)
            {
                GD.PrintErr("Cannot increase health by negative amount, converting to positive.");
                amount *= -1;
            }

            CurrentHealth += amount;
        }

        /// <summary>
        /// Decreases health by the specified amount. The amount should be
        /// positive. If a negative amount is provided, it will be
        /// automatically converted to a positive value. If the health is less
        /// than or equal to 0 afterward, execute the <see cref="Die"/> method.
        /// </summary>
        /// <param name="amount">The amount to decrease health by.</param>
        public virtual void TakeDamage(int amount)
        {
            if (amount < 0)
            {
                GD.PrintErr("Cannot decrease health by negative amount, converting to positive.");
                amount *= -1;
            }

            CurrentHealth -= amount;

            if (CurrentHealth <= 0)
            {
                HandleDeath();
            }
        }

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
