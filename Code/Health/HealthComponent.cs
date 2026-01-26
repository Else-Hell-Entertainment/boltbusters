using System;
using Godot;

namespace EHE.BoltBusters
{
    public abstract partial class HealthComponent : Node
    {
        /// <summary>
        /// Emitted when <see cref="CurrentHealth"/> changes.
        /// </summary>
        ///
        /// <param name="newHealth">
        /// The new value of <see cref="HealthComponent.CurrentHealth"/> after
        /// the change.
        /// </param>
        [Signal]
        public delegate void CurrentHealthChangedEventHandler(int newHealth);

        [Export]
        private int _maxHealth = 100;

        private int _currentHealth;
        private bool _isImmortal = false;

        /// <summary>
        /// The maximum health the entity can have.
        /// </summary>
        public int MaxHealth => _maxHealth;

        /// <summary>
        /// The current health of the entity.
        /// Clamped between 0 and <see cref="MaxHealth"/>.
        /// </summary>
        public int CurrentHealth
        {
            get => _currentHealth;
            protected set
            {
                _currentHealth = Math.Clamp(value, min: 0, max: _maxHealth);
                EmitSignal(SignalName.CurrentHealthChanged, _currentHealth);
            }
        }

        /// <summary>
        /// Is <see cref="CurrentHealth"/> greater than 0.
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>
        /// If the entity can take damage or not.
        /// </summary>
        public bool IsImmortal
        {
            get => _isImmortal;
            protected set => _isImmortal = value;
        }

        /// <summary>
        /// Increases the health by the given amount.
        /// </summary>
        ///
        /// <param name="amount">
        /// The amount to increase health by.
        /// </param>
        ///
        /// <remarks>
        /// The amount must be a positive integer. If a negative value is
        /// provided, it is automatically converted to a positive value.
        /// </remarks>
        public virtual void Increase(int amount)
        {
            if (amount < 0)
            {
                GD.PrintErr("Cannot increase health by negative amount, converting to positive.");
                amount *= -1;
            }

            CurrentHealth = Math.Clamp(CurrentHealth + amount, min: 0, max: MaxHealth);
        }

        /// <summary>
        /// Decreases the health by the given amount if <see cref="IsImmortal"/>
        /// is <b>not</b> set to <c>true</c>.
        /// </summary>
        ///
        /// <param name="amount">
        /// The amount to decrease health by.
        /// </param>
        ///
        /// <returns>
        /// <c>true</c> if the entity is still alive after taking damage,
        /// <c>false</c> otherwise.
        /// </returns>
        ///
        /// <remarks>
        /// The amount must be a positive integer. If a negative value is
        /// provided, it is automatically converted to a positive value.
        /// </remarks>
        public virtual bool Decrease(int amount)
        {
            if (!IsImmortal)
            {
                if (amount < 0)
                {
                    GD.PrintErr("Cannot decrease health by negative amount, converting to positive.");
                    amount *= -1;
                }

                CurrentHealth -= amount;
            }

            return IsAlive;
        }
    }
}
