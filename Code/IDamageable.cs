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
        /// Maximum health the damageable object can have.
        /// </summary>
        public int MaxHealth { get; set; }

        /// <summary>
        /// Current health of the object. Override setter to implement health
        /// clamping.
        /// </summary>
        public abstract int CurrentHealth { get; set; }

        /// <summary>
        /// Tells if the damageable object is currently alive or not.
        /// By default, returns <c>true</c> if <see cref="CurrentHealth"/> is
        /// greater than 0.
        /// </summary>
        public virtual bool IsAlive => CurrentHealth > 0;

        /// Increases health by the specified amount. The amount should be
        /// positive. If a negative amount is provided, it will be
        /// automatically converted to a positive value. By default, the health
        /// is capped to <see cref="MaxHealth"/>.
        /// </summary>
        /// <param name="amount">The amount to increase health by.</param>
        public virtual void Heal(int amount)
        {
            if (amount < 0)
            {
                GD.PrintErr("Cannot increase health by negative amount, converting to positive.");
                amount *= -1;
            }

            CurrentHealth = Math.Clamp(CurrentHealth + amount, min: 0, max: MaxHealth);
        }

        /// <summary>
        /// Decreases health by the specified amount. The amount should be
        /// positive. If a negative amount is provided, it will be
        /// automatically converted to a positive value. If the health is less
        /// than or equal to 0 afterward, execute the <see cref="HandleDeath"/> method.
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

            if (!IsAlive)
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
