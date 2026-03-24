// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System;
using EHE.Common.Godot;
using Godot;
using Godot.Collections;

namespace EHE.BoltBusters
{
    [GlobalClass]
    public partial class PlayerData : Resource, ISaveable
    {
        #region Constants

        private const string KEY_HEALTH = "Health";
        private const string KEY_COLLECTIBLE_COUNTS = "CollectibleCounts";
        private const string KEY_WEAPON_COUNTS = "WeaponCounts";
        private const string KEY_LEVEL_INDEX = "LevelIndex";
        private const string KEY_START_FROM_SHOP = "StartFromShop";

        private const string LOAD_ERROR_FORMAT = "Failed to load '{0}' from save data; using default value of '{1}'.";

        #endregion Constants


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
        private int _levelIndex = 1;

        // TODO: Read these from the default player data in GameManager!
        private static int s_defaultHealth = 100;
        private int _defaultLevelIndex = 1;
        private bool _defaultIsLevelCleared = false;
        private Dictionary<CollectibleType, int> _defaultCollectibleCounts = new()
        {
            { CollectibleType.Nut, 0 },
            { CollectibleType.Bolt, 0 },
            { CollectibleType.Wrench, 0 },
        };
        private Dictionary<WeaponType, int> _defaultWeaponCounts = new()
        {
            { WeaponType.Chaingun, 1 },
            { WeaponType.Railgun, 0 },
            { WeaponType.Rocket, 0 },
        };

        #endregion Private Fields


        #region Properties (private/protected/public)

        /// <summary>
        ///  Tells if the player is currently alive or not.
        /// </summary>
        public bool IsAlive => Health > 0;

        #endregion Properties (private/protected/public)


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
        [Export(PropertyHint.Range, "1,2147483647,1")]
        public int LevelIndex
        {
            get => _levelIndex;
            set => _levelIndex = Mathf.Clamp(value, min: 1, max: int.MaxValue);
        }

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
        ///  Increases the amount of the given collectible type by the given
        ///  amount. If no amount is provided, increases the value by 1.
        /// </summary>
        ///
        /// <param name="collectibleType">
        ///  The collectible whose count should be increased.
        /// </param>
        /// <param name="increment">
        ///  How much the amount should increase. Default is 1.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if increasing the amount was successful,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool IncreaseCollectibleAmount(CollectibleType collectibleType, int increment = 1)
        {
            var current = GetCollectibleAmount(collectibleType);

            // Invalid collectible type.
            if (current < 0)
            {
                return false;
            }

            // Invalid increment.
            if (increment < 0)
            {
                GD.PrintErr($"Cannot increase collectible amount by a negative value ({increment}).");
                return false;
            }

            return SetCollectibleAmount(collectibleType, amount: current + increment);
        }

        /// <summary>
        ///  Decreases the amount of the given collectible type by the given
        ///  amount. If no amount is provided, decreases the value by 1.
        /// </summary>
        ///
        /// <param name="collectibleType">
        ///  The collectible whose count should be increased.
        /// </param>
        /// <param name="decrement">
        ///  How much the amount should decrease. Default is 1.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if decreasing the amount was successful,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool DecreaseCollectibleAmount(CollectibleType collectibleType, int decrement = 1)
        {
            var current = GetCollectibleAmount(collectibleType);

            // Invalid collectible type.
            if (current < 0)
            {
                return false;
            }

            // Invalid decrement.
            if (decrement < 0)
            {
                GD.PrintErr($"Cannot decrease collectible amount by a negative value ({decrement}).");
                return false;
            }

            return SetCollectibleAmount(collectibleType, amount: current - decrement);
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
                GD.PushError($"Cannot set weapon count: key '{weaponType}' not found!");
                return false;
            }

            if (count < 0)
            {
                GD.PushError("Cannot set weapon count: count cannot be negative!");
                return false;
            }

            // TODO: Decide max value when designing UI.
            _weaponCounts[weaponType] = Mathf.Clamp(count, min: 0, max: int.MaxValue);
            EmitSignal(SignalName.CollectibleAmountsChanged, (int)weaponType, count);
            return true;
        }

        #endregion Public Methods


        #region ISaveable

        /// <summary>
        ///  Saves the following values to a Godot <see cref="Dictionary"/>:
        ///  <list type="bullet">
        ///   <item><see cref="LevelIndex"/></item>
        ///   <item><see cref="IsLevelCleared"/></item>
        ///   <item>number of each type of collectibles in possession</item>
        ///   <item>number of each type of weapon in possession</item>
        ///  </list>
        /// </summary>
        ///
        /// <returns>
        ///  <inheritdoc/>
        /// </returns>
        public Dictionary Save()
        {
            return new Dictionary()
            {
                [KEY_LEVEL_INDEX] = LevelIndex,
                [KEY_START_FROM_SHOP] = IsLevelCleared,
                [KEY_COLLECTIBLE_COUNTS] = _collectibleCounts,
                [KEY_WEAPON_COUNTS] = _weaponCounts,
            };
        }

        // TODO: Refactor this.
        /// <summary>
        ///  <inheritdoc/>
        ///  If reading the data fails, uses default values.
        ///  TODO: Provide defaults from GameManager!
        /// </summary>
        ///
        /// <param name="data">
        ///  <inheritdoc/>
        /// </param>
        public void Load(Dictionary data)
        {
            // Level index.
            if (
                !data.TryGetValue(KEY_LEVEL_INDEX, out var levelIndex)
                || (levelIndex.VariantType != Variant.Type.Float && levelIndex.VariantType != Variant.Type.Int)
                || (int)levelIndex < 1 // TODO: Set min level index in config.
            )
            {
                LevelIndex = _defaultLevelIndex; // TODO: Refactor, read from default player data resource.
                GD.PushError(string.Format(LOAD_ERROR_FORMAT, KEY_LEVEL_INDEX, LevelIndex));
            }
            else
            {
                LevelIndex = (int)levelIndex;
            }

            // Level cleared flag.
            if (
                !data.TryGetValue(KEY_START_FROM_SHOP, out var startFromShop)
                || startFromShop.VariantType != Variant.Type.Bool
            )
            {
                IsLevelCleared = _defaultIsLevelCleared; // TODO: Refactor, read from default player data resource.
                GD.PushError(string.Format(LOAD_ERROR_FORMAT, KEY_START_FROM_SHOP, IsLevelCleared));
            }
            else
            {
                IsLevelCleared = (bool)startFromShop;
            }

            // Collectible counts.
            if (
                !data.TryGetValue(KEY_COLLECTIBLE_COUNTS, out var collectibleCounts)
                || collectibleCounts.VariantType != Variant.Type.Dictionary
                || ((Dictionary)collectibleCounts).Count != 3 // TODO: Get dict length from default dict.
            )
            {
                _collectibleCounts = _defaultCollectibleCounts; // TODO: Refactor, read from default player data
                GD.PushError(string.Format(LOAD_ERROR_FORMAT, KEY_COLLECTIBLE_COUNTS, _collectibleCounts.Values));
            }
            else
            {
                _collectibleCounts = (Dictionary<CollectibleType, int>)collectibleCounts;
            }

            // Weapon counts.
            if (
                !data.TryGetValue(KEY_WEAPON_COUNTS, out var weaponCounts)
                || weaponCounts.VariantType != Variant.Type.Dictionary
                || ((Dictionary)weaponCounts).Count != 3 // TODO: Get dict length from default dict.
            )
            {
                _weaponCounts = _defaultWeaponCounts; // TODO: Refactor, read from default player data resource.
                GD.PushError(string.Format(LOAD_ERROR_FORMAT, KEY_WEAPON_COUNTS, _weaponCounts.Values));
            }
            else
            {
                _weaponCounts = (Dictionary<WeaponType, int>)weaponCounts;
            }
        }

        #endregion ISaveable
    }
}
