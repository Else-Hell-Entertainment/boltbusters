using System;
using Godot;

namespace EHE.BoltBusters
{
    [GlobalClass]
    public abstract partial class HealthComponent : Node
    {
        #region Signals
        // MARK: Signals

        /// <summary>
        ///  Emitted when <see cref="CurrentHealth"/> changes.
        /// </summary>
        ///
        /// <param name="newHealth">
        ///  The new value of <see cref="HealthComponent.CurrentHealth"/> after
        ///  the change.
        /// </param>
        [Signal]
        public delegate void CurrentHealthChangedEventHandler(int oldHealth, int newHealth);

        #endregion Signals


        #region Fields
        // MARK: Fields

        private int _currentHealth;

        #endregion Fields


        #region Properties
        // MARK: Properties

        /// <summary>
        ///  The maximum health the entity can have.
        /// </summary>
        [Export(PropertyHint.Range, "0,100,or_greater")]
        public int MaxHealth { get; protected set; } = 100;

        /// <summary>
        ///  The amount of health the entity had when it was added to the node
        ///  scene.
        /// </summary>
        [Export(PropertyHint.Range, "0,100,or_greater")]
        public int InitialHealth { get; protected set; } = 100;

        /// <summary>
        /// The current health of the entity.
        /// Clamped between 0 and <see cref="MaxHealth"/>.
        /// Emits the <see cref="CurrentHealthChanged"/> signal when the value
        /// changes.
        /// </summary>
        public int CurrentHealth
        {
            get => _currentHealth;
            protected set
            {
                int oldHealth = _currentHealth;
                _currentHealth = Math.Clamp(value, min: 0, max: MaxHealth);

                if (_currentHealth != oldHealth)
                {
                    EmitSignal(SignalName.CurrentHealthChanged, oldHealth, _currentHealth);
                }
            }
        }

        /// <summary>
        ///  Equivalent to <c><see cref="CurrentHealth"/> > 0</c>.
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>
        ///  If the entity can take damage or not.
        /// </summary>
        [Export]
        public bool IsImmortal { get; protected set; }

        #endregion Properties


        #region Public Methods
        // MARK: Public Methods

        /// <summary>
        ///  Initializes <see cref="CurrentHealth"/> with the initial value set
        ///  in inspector.
        /// </summary>
        public override void _Ready()
        {
            CurrentHealth = InitialHealth;
            GD.Print($"CurrentHealth initialized to {CurrentHealth}.");
        }

        /// <summary>
        ///  Increases <see cref="CurrentHealth"/> by the given amount.
        /// </summary>
        ///
        /// <param name="amount">The amount to increase health by.</param>
        ///
        /// <remarks>
        ///  The amount must be a positive integer. If a negative value is
        ///  provided, prints an error message and returns.
        /// </remarks>
        public virtual void Increase(int amount)
        {
            if (amount < 0)
            {
                GD.PrintErr($"Cannot increase health by negative amount ({amount}).");
                return;
            }

            CurrentHealth += amount;
        }

        /// <summary>
        ///  Decreases <see cref="CurrentHealth"/> byt the given amount if
        ///  applicable. The given amount must be positive.
        /// </summary>
        ///
        /// <param name="amount">The amount to decrease health by.</param>
        ///
        /// <returns>
        ///  <c>true</c> if damage was taken,
        ///  <c>false</c> if <see cref="IsImmortal"/> is set to <c>true</c> OR
        ///  if the given amount is negative.
        /// </returns>
        public virtual bool Decrease(int amount)
        {
            if (IsImmortal)
            {
                return false;
            }

            if (amount < 0)
            {
                GD.PrintErr($"Cannot decrease health by negative amount ({amount}).");
                return false;
            }

            CurrentHealth -= amount;
            return true;
        }

        /// <summary>
        ///  Sets the <see cref="CurrentHealth"/> back to
        ///  <see cref="InitialHealth"/>.
        /// </summary>
        public virtual void RestoreToInitial()
        {
            CurrentHealth = InitialHealth;
        }

        #endregion Public Methods
    }
}
