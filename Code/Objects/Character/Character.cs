using Godot;

namespace EHE.BoltBusters
{
    public abstract partial class Character : CharacterBody3D, IDamageable
    {
        [Export]
        private HealthComponent _healthComponent = null;

        public virtual void Heal(int amount)
        {
            _healthComponent.Increase(amount);
        }

        /// <summary>
        /// Handles taking damage. By default, this accounts only for the
        /// amount of damage defined by <paramref name="damageData"/> and
        /// applies it to the character's health component. Additionally, if the
        /// character's is considered dead after applying damage, executes the
        /// <see cref="HandleDeath"/> method.
        /// </summary>
        /// <param name="damageData">Information about the damage that was dealt.</param>
        public virtual void TakeDamage(DamageData damageData)
        {
            _healthComponent.Decrease(damageData.Amount);

            if (!_healthComponent.IsAlive)
            {
                HandleDeath();
            }
        }

        /// <summary>
        /// <b>WIP!</b>
        /// Handles the death of a character. By default, this simply deletes
        /// the character node from the scene using its <see cref="Node.QueueFree"/>
        /// method.
        /// </summary>
        public void HandleDeath()
        {
            QueueFree();
        }
    }
}
