using Godot;

namespace EHE.BoltBusters
{
    public abstract partial class Character : CharacterBody3D, IDamageable, ISpawnable
    {
        [Export]
        private PackedScene _deathAnimation;

        [Export]
        private HealthComponent _healthComponent = null;

        protected HealthComponent HealthComponent => _healthComponent;

        private DamageData _lastDamageData = new DamageData();

        /// <summary>
        /// Increases the character's health by the given <paramref name="amount"/>.
        /// </summary>
        /// <param name="amount"></param>
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
            _lastDamageData = damageData;

            if (!_healthComponent.IsAlive)
            {
                HandleDeath();
            }
        }

        public int GetCurrentHealth()
        {
            return _healthComponent.CurrentHealth;
        }

        /// <summary>
        /// Called when the character is spawned into the scene.
        /// Use this to initialize state, reset health,
        /// or perform any setup required before gameplay begins.
        /// </summary>
        public abstract void OnSpawn();

        /// <summary>
        /// <b>WIP!</b>
        /// Handles the death of a character independently from despawn logic.
        /// This allows death-related events to trigger only on actual death,
        /// not when an entity is forcefully despawned (e.g., during level cleanup).
        /// By default, this simply deletes the character node from the scene
        /// using its <see cref="Node.QueueFree"/> method.
        /// </summary>
        public virtual void HandleDeath()
        {
            if (_deathAnimation != null)
            {
                DeathAnimation animation = _deathAnimation.Instantiate<DeathAnimation>();
                LevelManager.Active.AddLevelObject(animation);
                animation.GlobalPosition = GlobalPosition;
                animation.DamageType = _lastDamageData.Type;
                Vector3 direction = GlobalPosition - LevelManager.Active.Player.GlobalPosition;
                animation.PlayDeathAnimation(direction);
            }
            OnDespawn();
        }

        /// <summary>
        /// Called when the character is to be removed from the scene.
        /// Use this to clean up timers, animations, effects,
        /// or return the character to an object pool.
        /// By default, this simply deletes the character node from the scene
        /// using its <see cref="Node.QueueFree"/> method.
        /// </summary>
        public virtual void OnDespawn()
        {
            QueueFree();
        }
    }
}
