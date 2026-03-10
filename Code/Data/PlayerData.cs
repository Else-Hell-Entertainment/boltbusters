// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System;
using Godot;
using Godot.Collections;

namespace EHE.BoltBusters
{
    [GlobalClass]
    public partial class PlayerData : Resource
    {
        #region Signals

        /// <summary>
        ///  Emitted when the <see cref="Health"/> property changes.
        /// </summary>
        ///
        /// <param name="newHealth">
        ///  The new value of <see cref="Health"/>.
        /// </param>
        [Signal]
        public delegate void HealthChangedEventHandler(int newHealth);

        /// <summary>
        ///  Emitted when the number of collected items changes.
        /// </summary>
        ///
        /// <param name="collectibleType">
        ///  Type of the collectible whose count changed.
        /// </param>
        /// <param name="newAmount">
        ///  The new count of the given type of collectible.
        /// </param>
        ///
        /// <seealso cref="CollectibleType"/>
        [Signal]
        public delegate void CollectibleAmountsChangedEventHandler(int collectibleType, int newAmount);

        /// <summary>
        ///  Emitted when the number of weapons changes.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  The integer value of the type of the weapon whose count changed.
        ///  See <see cref="WeaponType"/>.
        /// </param>
        /// <param name="newCount">
        ///  The new number of weapons of the given type.
        /// </param>
        ///
        /// <seealso cref="WeaponType"/>
        /// <seealso cref="SetNumberOfWeapons"/>
        /// <seealso cref="GetNumberOfWeapons"/>
        [Signal]
        public delegate void NumberOfWeaponsChangedEventHandler(int weaponType, int newCount);

        #endregion Signals


        #region Fields (private/protected)

        private int _health = 100;

        #endregion Private Fields


        #region Exported Fields & Properties (private/protected/public)

        [Export]
        private Dictionary<CollectibleType, int> _collectibleCounts = new()
        {
            { CollectibleType.Nut, 0 },
            { CollectibleType.Bolt, 0 },
            { CollectibleType.Wrench, 0 },
        };

        [Export]
        private Dictionary<WeaponType, int> _weaponCounts = new()
        {
            { WeaponType.Chaingun, 1 },
            { WeaponType.Railgun, 0 },
            { WeaponType.Rocket, 0 },
        };

        /// <summary>
        ///  The current health of the player.
        /// </summary>
        ///
        /// <remarks>
        ///  The <paramref name="value"/> is clamped between 0 and
        ///  <see cref="int.MaxValue"/>. <b>Note</b>: The maximum value will
        ///  be lowered when the UI is implemented.
        /// </remarks>
        [Export(PropertyHint.Range, "0,2147483647,1")]
        public int Health
        {
            get => _health;
            set
            {
                // TODO: Decide max value when designing UI.
                _health = Mathf.Clamp(value, min: 0, max: int.MaxValue);
                EmitSignal(SignalName.HealthChanged, _health);
            }
        }

        /// <summary>
        ///  The index of the level the player is currently in.
        /// </summary>
        [Export(PropertyHint.Range, "0,2147483647,1")]
        public int LevelIndex { get; set; } = 1;

        /// <summary>
        ///  Whether the player has already cleared the current level or not,
        ///  useful when loading data from a save game.
        /// </summary>
        ///
        /// <remarks>
        ///  This flag tells the save system whether to put the player at the
        ///  start of the level or in the shop state that is accessible after
        ///  the level has been cleared.
        /// </remarks>
        [Export]
        public bool IsLevelCleared { get; set; } = false;

        #endregion Exported Fields & Properties (private/protected/public)


        #region Public Methods

        /// <summary>
        ///  Gets the current amount of the specified collectible.
        /// </summary>
        ///
        /// <param name="collectibleType">
        ///  The type of collectible to query.
        /// </param>
        ///
        /// <returns>
        ///  The amount of the specified collectible type, or <c>-1</c> if the
        ///  given collectible type is invalid.
        /// </returns>
        ///
        /// <seealso cref="CollectibleType"/>
        /// <seealso cref="SetCollectibleAmount"/>
        public int GetCollectibleAmount(CollectibleType collectibleType)
        {
            if (!_collectibleCounts.TryGetValue(collectibleType, out var amount))
            {
                return -1;
            }

            return amount;
        }

        /// <summary>
        ///  Sets the amount of the specified collectible type.
        /// </summary>
        ///
        /// <param name="collectibleType">
        ///  The type of collectible to set the amount of.
        /// </param>
        /// <param name="amount">
        ///  The new amount for the collectible. Must be non-negative.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if the collectible amount was successfully set;
        ///  <c>false</c> if the collectible type is invalid or if the amount
        ///  is negative.
        /// </returns>
        ///
        /// <remarks>
        ///  <para>
        ///   This method emits the <see cref="CollectibleAmountsChanged"/>
        ///   signal when the amount is successfully updated.
        ///  </para>
        ///  <para>
        ///   The <paramref name="amount"/> is clamped between 0 and
        ///   <see cref="int.MaxValue"/>. <b>Note</b>: The maximum value will
        ///   be lowered when the UI is implemented.
        ///  </para>
        /// </remarks>
        ///
        /// <seealso cref="CollectibleType"/>
        /// <seealso cref="GetCollectibleAmount"/>
        /// <seealso cref="CollectibleAmountsChanged"/>
        public bool SetCollectibleAmount(CollectibleType collectibleType, int amount)
        {
            if (!_collectibleCounts.ContainsKey(collectibleType))
            {
                GD.PushError($"Cannot set collectible amount: key '{collectibleType}' not found!");
                return false;
            }

            if (amount < 0)
            {
                GD.PushError("Cannot set collectible amount: amount cannot be negative!");
                return false;
            }

            // TODO: Decide max value when designing UI.
            _collectibleCounts[collectibleType] = Mathf.Clamp(amount, min: 0, max: int.MaxValue);
            EmitSignal(SignalName.CollectibleAmountsChanged, (int)collectibleType, amount);
            return true;
        }

        /// <summary>
        ///  Gets the current number of the specified weapons.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  The type of weapon to query.
        /// </param>
        ///
        /// <returns>
        ///  The amount of the specified weapon type, or <c>-1</c> if the
        ///  given collectible type is invalid.
        /// </returns>
        ///
        /// <seealso cref="WeaponType"/>
        /// <seealso cref="SetNumberOfWeapons"/>
        public int GetNumberOfWeapons(WeaponType weaponType)
        {
            if (!_weaponCounts.TryGetValue(weaponType, out var amount))
            {
                return -1;
            }

            return amount;
        }

        /// <summary>
        ///  Sets the number of the specified weapons.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  The type of weapon to set the number of.
        /// </param>
        /// <param name="count">
        ///  The new count for the weapon. Must be non-negative.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if the number of weapons was successfully set;
        ///  <c>false</c> if the weapon type is invalid or if the count
        ///  is negative.
        /// </returns>
        ///
        /// <remarks>
        ///  <para>
        ///   This method emits the <see cref="NumberOfWeaponsChanged"/>
        ///   signal when the number of weapons is successfully updated.
        ///  </para>
        ///  <para>
        ///   The <paramref name="count"/> is clamped between 0 and
        ///   <see cref="int.MaxValue"/>. <b>Note</b>: The maximum value will
        ///   be set properly when this feature is fully implemented!
        ///  </para>
        /// </remarks>
        ///
        /// <seealso cref="WeaponType"/>
        /// <seealso cref="GetNumberOfWeapons"/>
        /// <seealso cref="NumberOfWeaponsChanged"/>
        public bool SetNumberOfWeapons(WeaponType weaponType, int count)
        {
            if (!_weaponCounts.ContainsKey(weaponType))
            {
                GD.PushError($"Cannot set collectible amount: key '{weaponType}' not found!");
                return false;
            }

            if (count < 0)
            {
                GD.PushError("Cannot set collectible amount: amount cannot be negative!");
                return false;
            }

            // TODO: Decide max value when designing UI.
            _weaponCounts[weaponType] = Mathf.Clamp(count, min: 0, max: int.MaxValue);
            EmitSignal(SignalName.CollectibleAmountsChanged, (int)weaponType, count);
            return true;
        }

        #endregion Public Methods
    }
}
