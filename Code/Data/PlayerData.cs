// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System;
using EHE.Common.Godot;
using EHE.Common.Godot.Logging;
using Godot;
using Godot.Collections;

namespace EHE.BoltBusters
{
    /// <summary>
    ///  Manages and persists player stats (including health, level
    ///  progression, collectibles, and weapons) throughout the game session.
    ///  This class serves as the primary data container for player information
    ///  and implements save/load functionality through the
    ///  <see cref="ISaveable"/> interface.
    /// </summary>
    ///
    /// <remarks>
    ///  <para>
    ///   PlayerData is a Godot Resource that can be serialized and exported to
    ///   the Godot editor. It signals changes to health, collectibles, and
    ///   weapons, allowing other systems to react to player state changes.
    ///  </para>
    ///  <para>
    ///   All collectible and weapon counts are stored in dictionaries and are
    ///   automatically clamped to valid ranges (0 to <see cref="int.MaxValue"/>).
    ///   Health is similarly constrained and should not exceed the maximum
    ///   value set in UI design.
    ///  </para>
    /// </remarks>
    ///
    /// <seealso cref="ISaveable"/>
    /// <seealso cref="CollectibleType"/>
    /// <seealso cref="WeaponType"/>
    [GlobalClass]
    public partial class PlayerData : Resource, ISaveable
    {
        #region Constants

        private const string KEY_COLLECTIBLE_COUNTS = "CollectibleCounts";
        private const string KEY_WEAPON_COUNTS = "WeaponCounts";
        private const string KEY_LEVEL_INDEX = "LevelIndex";
        private const string KEY_START_FROM_SHOP = "StartFromShop";
        private const string KEY_SECONDARY_UPGRADE_COUNTS = "SecondaryUpgradeCounts";
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
        ///  Emitted when the <see cref="LevelIndex"/> property
        ///  changes.
        /// </summary>
        ///
        /// <param name="newLevelIndex">
        ///  The new value of <see cref="LevelIndex"/>.
        /// </param>
        [Signal]
        public delegate void LevelIndexChangedEventHandler(int newLevelIndex);

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
        public delegate void CollectibleCountChangedEventHandler(int collectibleType, int newAmount);

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
        /// <seealso cref="PlayerData.SetWeaponCount"/>
        /// <seealso cref="PlayerData.GetWeaponCount"/>
        [Signal]
        public delegate void WeaponCountChangedEventHandler(int weaponType, int newCount);

        /// <summary>
        ///  Emitted when the number of secondary upgrades changes.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  The type of affected weapon as an integer.
        /// </param>
        /// <param name="newCount">
        ///  The new number of secondary upgrades.
        /// </param>
        [Signal]
        public delegate void SecondaryUpgradeCountChangedEventHandler(int weaponType, int newCount);

        #endregion Signals


        #region Default Values

        // Note: These values are updated to match those defined in the default
        // player data resource when the game is initialized!

        private static int s_defaultHealth = 100;
        private static int s_defaultLevelIndex = 1;
        private static bool s_defaultStartFromShop = false;
        private static Dictionary<CollectibleType, int> s_defaultCollectibleCounts = new()
        {
            { CollectibleType.Nut, 0 },
            { CollectibleType.Bolt, 0 },
            { CollectibleType.Wrench, 0 },
        };
        private static Dictionary<WeaponType, int> s_defaultWeaponCounts = new()
        {
            { WeaponType.Chaingun, 1 },
            { WeaponType.Railgun, 0 },
            { WeaponType.Rocket, 0 },
        };
        private static Dictionary<WeaponType, int> s_defaultSecondaryUpgradeCounts = new()
        {
            { WeaponType.Chaingun, 0 },
            { WeaponType.Railgun, 0 },
            { WeaponType.Rocket, 0 },
        };

        #endregion Default Values


        #region Fields

        private int _health = 100;
        private int _levelIndex = 1;

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

        [Export]
        private Dictionary<WeaponType, int> _secondaryUpgradeCounts = new()
        {
            { WeaponType.Chaingun, 0 },
            { WeaponType.Railgun, 0 },
            { WeaponType.Rocket, 0 },
        };

        #endregion Fields


        #region Properties

        /// <summary>
        ///  The current health of the player.
        /// </summary>
        ///
        /// <remarks>
        ///  <list type="bullet">
        ///   <item>
        ///    The values are automatically clamped between 0 and
        ///    <see cref="int.MaxValue"/>.
        ///   </item>
        ///   <item>
        ///    When health reaches 0, the player is considered dead (see
        ///    <see cref="IsAlive"/>).
        ///   </item>
        ///   <item>
        ///    The maximum value will be adjusted during UI implementation.
        ///   </item>
        ///   <item>
        ///    Changing this property emits the <see cref="HealthChanged"/>
        ///    signal.
        ///   </item>
        ///  </list>
        /// </remarks>
        ///
        /// <seealso cref="HealthChanged"/>
        /// <seealso cref="IsAlive"/>
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
        ///  Tells if the player is currently alive or not.
        /// </summary>
        public bool IsAlive => Health > 0;

        /// <summary>
        ///  The index of the level the player is currently in.
        /// </summary>
        ///
        /// <remarks>
        ///  Must be at least 1. The value is clamped between 1 and
        ///  <see cref="int.MaxValue"/>.
        /// </remarks>
        [Export(PropertyHint.Range, "1,2147483647,1")]
        public int LevelIndex
        {
            get => _levelIndex;
            set
            {
                _levelIndex = Mathf.Clamp(value, min: 1, max: int.MaxValue);
                EmitSignal(SignalName.LevelIndexChanged, _levelIndex);
            }
        }

        /// <summary>
        ///  Whether the player should start in shop when the save game is
        ///  loaded.
        /// </summary>
        ///
        /// <remarks>
        ///  When true, the game will load directly into the shop instead of
        ///  the main level. This is typically set to true after the player
        ///  completes a level successfully.
        /// </remarks>
        [Export]
        public bool StartFromShop { get; set; } = false;

        #endregion Properties


        #region Public Methods

        /// <summary>
        ///  <para>
        ///  Initializes internal default values from the using the provided
        ///  PlayerData object as reference.
        ///  </para>
        ///  <para>
        ///   <b>NOTE</b>: This method should be called only once during the
        ///   lifecycle of the game - right after the default player data
        ///   resource is loaded from the file!
        ///  </para>
        /// </summary>
        ///
        /// <remarks>
        ///  These default values are used as fallbacks during the
        ///  <see cref="Load(Dictionary)"/> operation when deserializing
        ///  player data fails or contains missing/invalid values.
        /// </remarks>
        ///
        /// <seealso cref="GameManager"/>
        /// <seealso cref="GameManager.DefaultPlayerData"/>
        /// <seealso cref="Load(Dictionary)"/>
        public static void UpdateDefaultValues(PlayerData defaultPlayerData)
        {
            if (defaultPlayerData == null)
            {
                GD.PushError("Failed to fetch default player data values, using hardcoded defaults instead.");
                return;
            }

            s_defaultHealth = defaultPlayerData.Health;
            s_defaultLevelIndex = defaultPlayerData.LevelIndex;
            s_defaultStartFromShop = defaultPlayerData.StartFromShop;
            s_defaultCollectibleCounts = defaultPlayerData._collectibleCounts;
            s_defaultWeaponCounts = defaultPlayerData._weaponCounts;
            s_defaultSecondaryUpgradeCounts = defaultPlayerData._secondaryUpgradeCounts;
        }

        // MARK: Collectible counts.
        /// <summary>
        ///  Returns all collectible counts as a dictionary where
        ///  <see cref="CollectibleType"/>s are the keys and the amounts are
        ///  the values.
        /// </summary>
        ///
        /// <returns>
        ///  A duplicate of the internal weapon counts dictionary.
        ///  The returned dictionary is a copy and modifications will not
        ///  affect the internal state; use <see cref="SetCollectibleCount"/>
        ///  or <see cref="IncreaseCollectibleCount"/> to modify collectible
        ///  counts.
        /// </returns>
        ///
        /// <seealso cref="GetCollectibleCount"/>
        public Dictionary<CollectibleType, int> GetCollectibleCounts()
        {
            return _collectibleCounts.Duplicate();
        }

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
        /// <remarks>
        ///  Pushes an error to the console if the given
        ///  <paramref name="collectibleType"/> cannot be found in the internal
        ///  dictionary.
        /// </remarks>
        ///
        /// <seealso cref="CollectibleType"/>
        /// <seealso cref="SetCollectibleCount"/>
        /// <seealso cref="GetCollectibleCounts"/>
        public int GetCollectibleCount(CollectibleType collectibleType)
        {
            if (!_collectibleCounts.TryGetValue(collectibleType, out var amount))
            {
                this.LogError($"Key not found '{collectibleType}'");
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
        ///  <c>true</c> if the collectible count was successfully set;
        ///  <c>false</c> if the collectible type is invalid or if the amount
        ///  is negative.
        /// </returns>
        ///
        /// <remarks>
        ///  <para>
        ///   This method emits the <see cref="CollectibleCountChanged"/>
        ///   signal when the amount is successfully updated.
        ///  </para>
        ///  <para>
        ///   The <paramref name="amount"/> is clamped between 0 and
        ///   <see cref="int.MaxValue"/>. The maximum value will be lowered
        ///   when the UI is fully implemented.
        ///  </para>
        /// </remarks>
        ///
        /// <seealso cref="CollectibleType"/>
        /// <seealso cref="GetCollectibleCount"/>
        /// <seealso cref="IncreaseCollectibleCount"/>
        /// <seealso cref="DecreaseCollectibleCount"/>
        /// <seealso cref="CollectibleCountChanged"/>
        private bool SetCollectibleCount(CollectibleType collectibleType, int amount)
        {
            if (!_collectibleCounts.ContainsKey(collectibleType))
            {
                GD.PushError($"Cannot set collectible count: key '{collectibleType}' not found!");
                return false;
            }

            if (amount < 0)
            {
                GD.PushError("Cannot set collectible count: amount cannot be negative!");
                return false;
            }

            // TODO: Decide max value when designing UI.
            _collectibleCounts[collectibleType] = Mathf.Clamp(amount, min: 0, max: int.MaxValue);
            EmitSignal(SignalName.CollectibleCountChanged, (int)collectibleType, amount);
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
        ///  Must be non-negative.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if increasing the amount was successful,
        ///  <c>false</c> if the collectible type is invalid or if the
        ///  increment is negative.
        /// </returns>
        ///
        /// <remarks>
        ///  Emits the <see cref="CollectibleCountChanged"/> signal on success.
        /// </remarks>
        ///
        /// <seealso cref="DecreaseCollectibleCount"/>
        /// <seealso cref="SetCollectibleCount"/>
        public bool IncreaseCollectibleCount(CollectibleType collectibleType, int increment = 1)
        {
            var current = GetCollectibleCount(collectibleType);

            // Invalid collectible type.
            if (current < 0)
            {
                this.LogError($"Cannot increase collectible count: invalid collectible type '{collectibleType}'!");
                return false;
            }

            // Invalid increment.
            if (increment < 0)
            {
                this.LogError($"Cannot increase collectible count: increment cannot be negative!");
                return false;
            }

            return SetCollectibleCount(collectibleType, amount: current + increment);
        }

        /// <summary>
        ///  Decreases the amount of the given collectible type by the given
        ///  amount. If no amount is provided, decreases the value by 1.
        /// </summary>
        ///
        /// <param name="collectibleType">
        ///  The collectible whose count should be decreased.
        /// </param>
        /// <param name="decrement">
        ///  How much the amount should decrease. Default is 1.
        ///  Must be non-negative.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if decreasing the amount was successful,
        ///  <c>false</c> if the collectible type is invalid, the decrement
        ///  is negative, or the decrement would make the value negative.
        /// </returns>
        ///
        /// <remarks>
        ///  Emits the <see cref="CollectibleCountChanged"/> signal on success.
        /// </remarks>
        ///
        /// <seealso cref="IncreaseCollectibleCount"/>
        /// <seealso cref="SetCollectibleCount"/>
        public bool DecreaseCollectibleCount(CollectibleType collectibleType, int decrement = 1)
        {
            var current = GetCollectibleCount(collectibleType);

            // Invalid collectible type.
            if (current < 0)
            {
                this.LogError($"Cannot decrease collectible count: invalid collectible type '{collectibleType}'!");
                return false;
            }

            // Invalid decrement.
            if (decrement < 0)
            {
                this.LogError($"Cannot decrease collectible count: decrement cannot be negative!");
                return false;
            }

            return SetCollectibleCount(collectibleType, amount: current - decrement);
        }

        // MARK: Primary weapon upgrades (weapon counts).
        /// <summary>
        ///  Returns all weapon counts as a dictionary where
        ///  <see cref="WeaponType"/>s are the keys and the number of weapons
        ///  are the values.
        /// </summary>
        ///
        /// <returns>
        ///  A duplicate of the internal weapon counts dictionary.
        ///  The returned dictionary is a copy and modifications will not
        ///  affect the internal state; use <see cref="SetWeaponCount"/> or
        ///  <see cref="IncreaseWeaponCount"/> to modify weapon counts.
        /// </returns>
        ///
        /// <seealso cref="GetWeaponCount"/>
        public Dictionary<WeaponType, int> GetWeaponCounts()
        {
            return _weaponCounts.Duplicate();
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
        ///  given weapon type is invalid.
        /// </returns>
        ///
        /// <seealso cref="WeaponType"/>
        /// <seealso cref="SetWeaponCount"/>
        /// <seealso cref="GetWeaponCounts"/>
        public int GetWeaponCount(WeaponType weaponType)
        {
            if (!_weaponCounts.TryGetValue(weaponType, out var amount))
            {
                this.LogError($"Key not found '{weaponType}'");
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
        ///   This method emits the <see cref="WeaponCountChanged"/>
        ///   signal when the number of weapons is successfully updated.
        ///  </para>
        ///  <para>
        ///   The <paramref name="count"/> is clamped between 0 and
        ///   <see cref="int.MaxValue"/>. The maximum value will be set
        ///   properly when this feature is fully implemented.
        ///  </para>
        /// </remarks>
        ///
        /// <seealso cref="WeaponType"/>
        /// <seealso cref="GetWeaponCount"/>
        /// <seealso cref="IncreaseWeaponCount"/>
        /// <seealso cref="DecreaseWeaponCount"/>
        public bool SetWeaponCount(WeaponType weaponType, int count)
        {
            if (!_weaponCounts.ContainsKey(weaponType))
            {
                this.LogError($"Cannot set weapon count: key '{weaponType}' not found!");
                return false;
            }

            if (count < 0)
            {
                this.LogError("Cannot set weapon count: count cannot be negative!");
                return false;
            }

            // TODO: Decide max value when designing UI.
            _weaponCounts[weaponType] = Mathf.Clamp(count, min: 0, max: int.MaxValue);
            EmitSignal(SignalName.WeaponCountChanged, (int)weaponType, count);
            return true;
        }

        /// <summary>
        ///  Increases the number of the weapons of the given type by the given
        ///  amount. If no amount is provided, increases the value by 1.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  The type of the weapon whose count should be increased.
        /// </param>
        /// <param name="increment">
        ///  How much the count should be increased by. Default is 1.
        ///  Must be non-negative.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if increasing the number of weapons was successful,
        ///  <c>false</c> if the weapon type is invalid or if the increment
        ///  is negative.
        /// </returns>
        ///
        /// <remarks>
        ///  Emits the <see cref="WeaponCountChanged"/> signal on success.
        /// </remarks>
        ///
        /// <seealso cref="DecreaseWeaponCount"/>
        /// <seealso cref="SetWeaponCount"/>
        public bool IncreaseWeaponCount(WeaponType weaponType, int increment = 1)
        {
            var current = GetWeaponCount(weaponType);

            // Weapon type not found.
            if (current < 0)
            {
                return false;
            }

            // Invalid increment.
            if (increment < 0)
            {
                this.LogError($"Cannot increase weapon count by a negative value ({increment}).");
                return false;
            }

            return SetWeaponCount(weaponType, count: current + increment);
        }

        /// <summary>
        ///  Decreases the number of the weapons of the given type by the given
        ///  amount. If no amount is provided, decreases the value by 1.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  The type of the weapon whose count should be decreased.
        /// </param>
        /// <param name="decrement">
        ///  How much the count should be decreased by. Default is 1.
        ///  Must be non-negative.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if decreasing the number of weapons was successful,
        ///  <c>false</c> if the weapon type is invalid, the decrement
        ///  is negative, or the decrement would make the value negative.
        /// </returns>
        ///
        /// <remarks>
        ///  Emits the <see cref="WeaponCountChanged"/> signal on success.
        /// </remarks>
        ///
        /// <seealso cref="IncreaseWeaponCount"/>
        /// <seealso cref="SetWeaponCount"/>
        public bool DecreaseWeaponCount(WeaponType weaponType, int decrement = 1)
        {
            var current = GetWeaponCount(weaponType);

            // Weapon type not found.
            if (current < 0)
            {
                return false;
            }

            // Invalid decrement.
            if (decrement < 0)
            {
                this.LogError($"Cannot decrease weapon count by a negative value ({decrement}).");
                return false;
            }

            return SetWeaponCount(weaponType, count: current - decrement);
        }

        // MARK: Secondary weapon upgrades.
        /// <summary>
        ///  Returns all secondary weapon upgrade counts as a dictionary where
        ///  <see cref="WeaponType"/>s are the keys and the number count of
        ///  upgrades are the values.
        /// </summary>
        ///
        /// <returns>
        ///  A duplicate of the internal secondary upgrade counts dictionary.
        ///  As the returned dictionary is a copy, modifications will NOT
        ///  affect the state of the internal state. Use
        ///  <see cref="SetSecondaryUpgradeCount"/>,
        ///  <see cref="IncreaseSecondaryUpgradeCount"/>, and
        ///  <see cref="DecreaseSecondaryUpgradeCount"/> to modify the values.
        /// </returns>
        public Dictionary<WeaponType, int> GetSecondaryUpgradeCounts()
        {
            return _secondaryUpgradeCounts.Duplicate();
        }

        /// <summary>
        ///  Gets the current count of secondary upgrades purchased for the
        ///  specified weapon.
        /// </summary>
        ///
        /// <param name="weaponType">
        ///  Type of the weapon to query.
        /// </param>
        ///
        /// <returns>
        ///  The number of secondary upgrades purchased or <c>-1</c> if the
        ///  given weapon type is invalid.
        /// </returns>
        ///
        /// <seealso cref="GetSecondaryUpgradeCounts"/>
        /// <seealso cref="SetSecondaryUpgradeCount"/>
        /// <seealso cref="IncreaseSecondaryUpgradeCount"/>
        /// <seealso cref="DecreaseSecondaryUpgradeCount"/>
        public int GetSecondaryUpgradeCount(WeaponType weaponType)
        {
            if (_secondaryUpgradeCounts.TryGetValue(weaponType, out var count))
            {
                return count;
            }

            this.LogError($"Cannot get secondary upgrade count: key '{weaponType}' not found!");
            return -1;
        }

        /// <summary>
        ///  Sets the number of secondary upgrades for the given weapon.
        /// </summary>
        ///
        /// <param name="weaponType">Type of the weapon affected.</param>
        /// <param name="count">The new count of secondary upgrades.</param>
        ///
        /// <returns>
        ///  <c>true</c> if the value was set successfully,
        ///  <c>false</c> if the weapon type is invalid or if the count
        ///  provided is negative.
        /// </returns>
        ///
        /// <remarks>
        ///  <para>
        ///   This method emits the <see cref="SecondaryUpgradeCountChanged"/>
        ///   signal when the value has been set successfully.
        ///  </para>
        ///  <para>
        ///   The value is clamped between 0 and <see cref="int.MaxValue"/>.
        ///   This does NOT reflect the actual maximum allowed count set by the
        ///   weapon controller itself. See the code for the controllers to
        ///   find out the actual maximums.
        ///  </para>
        /// </remarks>
        public bool SetSecondaryUpgradeCount(WeaponType weaponType, int count)
        {
            if (!_secondaryUpgradeCounts.ContainsKey(weaponType))
            {
                this.LogError($"Cannot set secondary upgrade count: key '{weaponType}' not found!");
                return false;
            }

            if (count < 0)
            {
                this.LogError("Cannot set secondary upgrade count: count cannot be negative!");
                return false;
            }

            // TODO: The max secondary count should be queried from the weapon controller!
            _secondaryUpgradeCounts[weaponType] = Mathf.Clamp(count, min: 0, max: int.MaxValue);
            EmitSignal(SignalName.SecondaryUpgradeCountChanged, (int)weaponType, count);
            return true;
        }

        /// <summary>
        ///  Increases the number of secondary upgrades for the given weapon
        ///  type. If no increment is specified, 1 is used by default.
        /// </summary>
        ///
        /// <param name="weaponType">The type of the affected weapon.</param>
        /// <param name="increment">
        ///  How many secondary upgrades should be added.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if the value is successfully increased,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool IncreaseSecondaryUpgradeCount(WeaponType weaponType, int increment = 1)
        {
            var current = GetSecondaryUpgradeCount(weaponType);

            // Weapon type not found.
            if (current < 0)
            {
                this.LogError($"Cannot increase secondary upgrade count: key not found '{weaponType}'!");
                return false;
            }

            // Invalid increment.
            if (increment < 0)
            {
                this.LogError($"Cannot increase secondary upgrade count: increment cannot be negative!");
                return false;
            }

            return SetWeaponCount(weaponType, count: current + increment);
        }

        /// <summary>
        ///  Decreases the number of secondary upgrades for the given weapon
        ///  type. If no decrement is specified, 1 is used by default.
        /// </summary>
        ///
        /// <param name="weaponType">The type of the affected weapon.</param>
        /// <param name="decrement">
        ///  How many secondary upgrades should be removed.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> if the value is successfully decreased,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool DecreaseSecondaryUpgradeCount(WeaponType weaponType, int decrement = 1)
        {
            var current = GetSecondaryUpgradeCount(weaponType);

            // Weapon type not found.
            if (current < 0)
            {
                this.LogError($"Cannot decrease secondary upgrade count: key not found '{weaponType}'!");
                return false;
            }

            // Invalid decrement.
            if (decrement < 0)
            {
                this.LogError($"Cannot decrease secondary upgrade count: decrement cannot be negative!");
                return false;
            }

            return SetSecondaryUpgradeCount(weaponType, count: current - decrement);
        }

        #endregion Public Methods


        #region ISaveable

        /// <summary>
        ///  Saves the following values to a Godot <see cref="Dictionary"/>:
        ///  <list type="bullet">
        ///   <item><see cref="LevelIndex"/></item>
        ///   <item><see cref="StartFromShop"/></item>
        ///   <item>number of each type of collectibles in possession</item>
        ///   <item>number of each type of weapon in possession</item>
        ///  </list>
        /// </summary>
        ///
        /// <returns>
        ///  A <see cref="Godot.Collections.Dictionary"/> containing the
        ///  serialized player data that can be persisted to storage.
        /// </returns>
        ///
        /// <remarks>
        ///  Note: Health is not saved as it is intended to be reset between
        ///  levels. This method is part of the <see cref="ISaveable"/>
        ///  interface implementation.
        /// </remarks>
        ///
        /// <seealso cref="Load"/>
        /// <seealso cref="ISaveable"/>
        public Dictionary Save()
        {
            return new Dictionary()
            {
                [KEY_LEVEL_INDEX] = LevelIndex,
                [KEY_START_FROM_SHOP] = StartFromShop,
                [KEY_COLLECTIBLE_COUNTS] = _collectibleCounts,
                [KEY_WEAPON_COUNTS] = _weaponCounts,
                [KEY_SECONDARY_UPGRADE_COUNTS] = _secondaryUpgradeCounts,
            };
        }

        /// <summary>
        ///  Loads player data from a <see cref="Godot.Collections.Dictionary"/>
        ///  dictionary. If reading the data fails, uses default values.
        /// </summary>
        ///
        /// <param name="data">
        ///  A <see cref="Godot.Collections.Dictionary"/> containing previously
        ///  saved player data. Expected to contain keys: <c>LevelIndex</c>,
        ///  <c>StartFromShop</c>, <c>CollectibleCounts</c>, and
        ///  <c>WeaponCounts</c>.
        /// </param>
        ///
        /// <remarks>
        ///  <para>
        ///   If any required data is missing or invalid, this method will use
        ///   default values and log an error message via
        ///   <see cref="Godot.GD.PushError(string)"/>.
        ///  </para>
        ///  <para>
        ///   This method is part of the <see cref="ISaveable"/> interface
        ///   implementation.
        ///  </para>
        /// </remarks>
        ///
        /// <seealso cref="Save"/>
        /// <seealso cref="ISaveable"/>
        public void Load(Dictionary data)
        {
            LoadLevelIndex(data);
            LoadLevelClearedFlag(data);
            LoadCollectibleCounts(data);
            LoadWeaponCounts(data);
            LoadSecondaryUpgradeCounts(data);
        }

        #endregion ISaveable


        #region Private Load Helpers

        /// <summary>
        ///  Attempts to load the level index from save data with validation.
        /// </summary>
        ///
        /// <param name="data">
        ///  The save data dictionary containing serialized player information.
        /// </param>
        ///
        /// <remarks>
        ///  If the level index is missing, invalid, or less than 1, this
        ///  method will set it to the default value and log an error message.
        ///  Valid level indices must be integers or floats and must be at
        ///  least 1.
        /// </remarks>
        ///
        /// <seealso cref="LevelIndex"/>
        private void LoadLevelIndex(Dictionary data)
        {
            if (
                !data.TryGetValue(KEY_LEVEL_INDEX, out var levelIndex)
                || (levelIndex.VariantType != Variant.Type.Float && levelIndex.VariantType != Variant.Type.Int)
                || (int)levelIndex < s_defaultLevelIndex
            )
            {
                LevelIndex = s_defaultLevelIndex;
                this.LogError(string.Format(LOAD_ERROR_FORMAT, KEY_LEVEL_INDEX, LevelIndex));
            }
            else
            {
                LevelIndex = (int)levelIndex;
            }
        }

        /// <summary>
        ///  Attempts to load the "start from shop" flag from save data with
        ///  validation.
        /// </summary>
        ///
        /// <param name="data">
        ///  The save data dictionary containing serialized player information.
        /// </param>
        ///
        /// <remarks>
        ///  If the flag is missing or not a boolean value, this method will
        ///  set it to the default value (<c>false</c>) and log an error
        ///  message.
        /// </remarks>
        ///
        /// <seealso cref="StartFromShop"/>
        private void LoadLevelClearedFlag(Dictionary data)
        {
            if (
                !data.TryGetValue(KEY_START_FROM_SHOP, out var startFromShop)
                || startFromShop.VariantType != Variant.Type.Bool
            )
            {
                StartFromShop = s_defaultStartFromShop;
                this.LogError(string.Format(LOAD_ERROR_FORMAT, KEY_START_FROM_SHOP, StartFromShop));
            }
            else
            {
                StartFromShop = (bool)startFromShop;
            }
        }

        /// <summary>
        ///  Attempts to load collectible counts from save data with validation.
        /// </summary>
        ///
        /// <param name="data">
        ///  The save data dictionary containing serialized player information.
        /// </param>
        ///
        /// <remarks>
        ///  If the collectible counts dictionary is missing, or invalid, this
        ///  method will reset to default values and log an error message. Each
        ///  entry is expected to have a <see cref="CollectibleType"/> key and
        ///  an integer count value.
        /// </remarks>
        ///
        /// <seealso cref="GetCollectibleCounts"/>
        /// <seealso cref="CollectibleType"/>
        private void LoadCollectibleCounts(Dictionary data)
        {
            if (
                !data.TryGetValue(KEY_COLLECTIBLE_COUNTS, out var collectibleCounts)
                || collectibleCounts.VariantType != Variant.Type.Dictionary
                || ((Dictionary)collectibleCounts).Count != s_defaultCollectibleCounts.Count
            )
            {
                _collectibleCounts = s_defaultCollectibleCounts.Duplicate();
                this.LogError(string.Format(LOAD_ERROR_FORMAT, KEY_COLLECTIBLE_COUNTS, _collectibleCounts.Values));
            }
            else
            {
                foreach (var (type, count) in (Dictionary)collectibleCounts)
                {
                    SetCollectibleCount((CollectibleType)(int)type, (int)count);
                }
            }
        }

        /// <summary>
        ///  Attempts to load weapon counts from save data with validation.
        /// </summary>
        ///
        /// <param name="data">
        ///  The save data dictionary containing serialized player information.
        /// </param>
        ///
        /// <remarks>
        ///  If the weapon counts dictionary is missing or invalid, this method
        ///  will reset to default values and log an error message. Each entry
        ///  is expected to have a <see cref="WeaponType"/> key and an integer
        ///  count value.
        /// </remarks>
        ///
        /// <seealso cref="GetWeaponCounts"/>
        /// <seealso cref="WeaponType"/>
        private void LoadWeaponCounts(Dictionary data)
        {
            if (
                !data.TryGetValue(KEY_WEAPON_COUNTS, out var weaponCounts)
                || weaponCounts.VariantType != Variant.Type.Dictionary
                || ((Dictionary)weaponCounts).Count != s_defaultWeaponCounts.Count
            )
            {
                _weaponCounts = s_defaultWeaponCounts.Duplicate();
                this.LogError(string.Format(LOAD_ERROR_FORMAT, KEY_WEAPON_COUNTS, _weaponCounts.Values));
            }
            else
            {
                foreach (var (type, count) in (Dictionary)weaponCounts)
                {
                    SetWeaponCount((WeaponType)(int)type, (int)count);
                }
            }
        }

        /// <summary>
        ///  Attempts to load secondary upgrade counts from save data with
        ///  validation.
        /// </summary>
        ///
        /// <param name="data">
        ///  The save data dictionary containing serialized player information.
        /// </param>
        ///
        /// <remarks>
        ///  If the secondary upgrade counts dictionary is missing or invalid,
        ///  this method will reset to default values and log an error message.
        ///  Each entry is expected to have a <see cref="WeaponType"/> key and
        ///  an integer count value.
        /// </remarks>
        ///
        /// <seealso cref="GetSecondaryUpgradeCounts"/>
        /// <seealso cref="WeaponType"/>
        private void LoadSecondaryUpgradeCounts(Dictionary data)
        {
            if (
                !data.TryGetValue(KEY_SECONDARY_UPGRADE_COUNTS, out var secondaryUpgradeCounts)
                || secondaryUpgradeCounts.VariantType != Variant.Type.Dictionary
                || ((Dictionary)secondaryUpgradeCounts).Count != s_defaultSecondaryUpgradeCounts.Count
            )
            {
                _secondaryUpgradeCounts = s_defaultSecondaryUpgradeCounts.Duplicate();
                this.LogError(
                    string.Format(LOAD_ERROR_FORMAT, KEY_SECONDARY_UPGRADE_COUNTS, _secondaryUpgradeCounts.Values)
                );
            }
            else
            {
                foreach (var (type, count) in (Dictionary)secondaryUpgradeCounts)
                {
                    SetSecondaryUpgradeCount((WeaponType)(int)type, (int)count);
                }
            }
        }

        #endregion Private Load Helpers
    }
}
