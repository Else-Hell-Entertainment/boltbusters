// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>
//            Miska Rihu <miska.rihu@tuni.fi>

using System.Collections.Generic;
using EHE.Common.Godot.Extensions;
using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Base class for a weapon group controller. Can accept a single type of weapon. IMPORTANT: for weapon slots to
    /// work, add any number of Node3D nodes as children of the WeaponSlots node in the editor. Weapons will be spawned
    /// to these points.
    /// </summary>
    public abstract partial class PlayerWeaponGroupController : Node3D, IAttacker, IUpgradeable
    {
        private List<Node3D> _weaponSlots = new List<Node3D>();

        [Export]
        private PackedScene _weaponScene;

        /// <summary>
        ///  Price info for each supported upgrades. <see cref="UpgradeType"/>
        ///  are used as keys and <see cref="PriceInfo"/> as values.
        /// </summary>
        [Export]
        protected Godot.Collections.Dictionary<UpgradeType, PriceInfo> Prices { get; set; }

        public List<BaseWeapon> Weapons { get; } = new List<BaseWeapon>();

        /// <summary>
        /// The maximum number of weapons the controller can hold.
        /// </summary>
        public int MaxWeaponCount => _weaponSlots.Count;

        /// <summary>
        /// The number of weapons currently equipped.
        /// </summary>
        public int CurrentWeaponCount => Weapons.Count;

        /// <summary>
        /// The type of weapons added to this controller.
        /// </summary>
        public virtual WeaponType WeaponType => WeaponType.None;

        public int SecondaryUpgradeCount { get; protected set; }

        /// <summary>
        ///  Maximum number of secondary upgrades for this weapon controller.
        /// </summary>
        public abstract int MaxSecondaryUpgradeCount { get; }

        public override void _Ready()
        {
            Node3D weaponSlots = GetNode<Node3D>("WeaponSlots");
            foreach (var node in weaponSlots.GetChildren())
            {
                if (node is Node3D node3D)
                {
                    _weaponSlots.Add(node3D);
                }
            }
        }

        /// <summary>
        /// Base implementation will attack with every weapon that is capable of attacking during the frame.
        /// Override this for more sophisticated attack patterns.
        /// </summary>
        public virtual void Attack()
        {
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon.CanAttack)
                {
                    weapon.Attack();
                }
            }
        }

        /// <summary>
        /// Add a new weapon of type BaseWeapon to the controller. Set the appropriate weapon scene in the editor.
        /// </summary>
        public virtual bool AddWeapon()
        {
            if (Weapons.Count >= _weaponSlots.Count)
            {
                this.LogDebug("Not enough slots!");
                return false;
            }

            var weapon = _weaponScene.Instantiate<BaseWeapon>();
            Weapons.Add(weapon);
            int newIndex = Weapons.Count - 1;
            Node3D node = _weaponSlots[newIndex];
            weapon.Position = node.GetPosition();
            AddChild(weapon);
            GameManager.Instance.EmitSignal(GameManager.SignalName.RequestHudRefresh);
            return true;
        }

        /// <summary>
        /// Removes a weapon from the last index of the controller's list (LIFO) and calls QueueFree on it.
        /// </summary>
        public virtual bool RemoveWeapon()
        {
            if (Weapons.Count > 0)
            {
                int lastIndex = Weapons.Count - 1;
                BaseWeapon weapon = Weapons[lastIndex];
                Weapons.RemoveAt(lastIndex);
                weapon.QueueFree();
                GameManager.Instance.EmitSignal(GameManager.SignalName.RequestHudRefresh);
                return true;
            }

            return false;
        }

        /// <summary>
        ///  Initializes the weapon controller by setting the weapon count.
        /// </summary>
        ///
        /// <param name="weaponCount">
        ///  The desired number of weapons.
        /// </param>
        ///
        /// <returns>
        ///  <c>true</c> initialization is performed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool Initialize(int weaponCount, int secondaryCount)
        {
            this.PrintDebug($"Initializing {WeaponType} controller");

            if (weaponCount > MaxWeaponCount)
            {
                this.LogError($"Cannot initialize controller for {WeaponType}, weapon count exceeded.");
                return false;
            }

            // Ensure that there are no weapons before adding new ones.
            if (CurrentWeaponCount > 0)
            {
                RemoveAllWeapons();
            }

            // Add new weapons.
            for (int i = 0; i < weaponCount; i++)
            {
                this.LogDebug($"Adding weapons {i + 1}/{weaponCount} ");
                AddWeapon();
            }

            SecondaryUpgradeCount = 0; // Makes sure that the counter is reset before initialization.
            InitializeSecondaryUpgrades(secondaryCount);

            UpdatePlayerData();
            return true;
        }

        /// <summary>
        /// Resets all weapons to their default state by calling BaseWeapon.Reset().
        /// If the controller has any custom behaviours that also need to be reset, override the method and add the
        /// mechanics. Note that base implementation for Reset() is empty and needs to be also implemented.
        /// </summary>
        public virtual void ResetWeapons()
        {
            foreach (BaseWeapon weapon in Weapons)
            {
                weapon.Reset();
            }
        }

        #region IUpgradeable

        /// <summary>
        ///  Returns the price for the given upgrade type or <c>null</c> if
        ///  info is not found.
        /// </summary>
        ///
        /// <param name="upgradeType"><inheritdoc/></param>
        ///
        /// <returns>
        ///  The price of the given upgrade as a <see cref="PriceInfo"/> object
        ///  if one is defined; <c>null</c> otherwise.
        /// </returns>
        ///
        /// <remarks>
        ///  This method logs an error message with the stack trace if finding
        ///  the upgrade type fails.
        /// </remarks>
        public PriceInfo GetPrice(UpgradeType upgradeType)
        {
            if (Prices.TryGetValue(upgradeType, out var priceInfo))
            {
                return priceInfo;
            }

            this.LogError($"Cannot get upgrade price for '{upgradeType}': Key not found!");
            return null;
        }

        /// <inheritdoc/>
        public bool SetPrice(UpgradeType upgradeType, PriceInfo priceInfo)
        {
            if (Prices.ContainsKey(upgradeType))
            {
                Prices[upgradeType] = priceInfo;
                return true;
            }

            this.LogError($"Cannot set upgrade price for '{upgradeType}': Key not found!");
            return false;
        }

        /// <inheritdoc/>
        public bool Upgrade(UpgradeType type)
        {
            var wasUpgraded = OnUpgrade(type);
            if (wasUpgraded)
            {
                UpdatePlayerData();
            }
            return wasUpgraded;
        }

        /// <inheritdoc/>
        public bool Downgrade(UpgradeType type)
        {
            var wasDowngraded = OnDowngrade(type);
            if (wasDowngraded)
            {
                UpdatePlayerData();
            }
            return wasDowngraded;
        }

        #endregion IUpgradeable


        #region Protected Implementations

        protected abstract bool InitializeSecondaryUpgrades(int secondaryCount);

        /// <summary>
        ///  Handles the actual upgrade logic for different upgrade types.
        /// </summary>
        ///
        /// <param name="upgradeType">Type of the upgrade to perform.</param>
        ///
        /// <returns>
        ///  <c>true</c> if upgrade was performed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        protected abstract bool OnUpgrade(UpgradeType upgradeType);

        /// <summary>
        ///  Handles the actual downgrade logic for different upgrade types.
        /// </summary>
        ///
        /// <param name="upgradeType">Type of the downgrade to perform.</param>
        ///
        /// <returns>
        ///  <c>true</c> if downgrade was performed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        protected abstract bool OnDowngrade(UpgradeType upgradeType);

        #endregion Protected Implementations


        #region Private Implementations

        /// <summary>
        ///  Removes all weapons from the controller.
        /// </summary>
        private void RemoveAllWeapons()
        {
            for (int i = CurrentWeaponCount; i > 0; i--)
            {
                this.LogDebug("Removing all weapons.");
                RemoveWeapon();
            }
        }

        /// <summary>
        ///  Updates the weapon status information to
        ///  <see cref="GameManager.CurrentPlayerData"/> if possible.
        /// </summary>
        private void UpdatePlayerData()
        {
            var playerData = GameManager.Instance.CurrentPlayerData;

            if (playerData == null)
            {
                this.LogWarning("Current player data is null.");
                return;
            }

            // TODO: Make sure this doesn't cause issues in case the values were the same as before!
            playerData.SetWeaponCount(WeaponType, CurrentWeaponCount);
            playerData.SetSecondaryUpgradeCount(WeaponType, SecondaryUpgradeCount);
        }

        #endregion Private Implementations
    }
}
