using System;
using EHE.Common.Godot.Extensions;
using Godot;

namespace EHE.BoltBusters
{
    public partial class Player : Character
    {
        private PlayerChaingunController _chaingunController;
        private PlayerRailgunController _railgunController;
        private PlayerRocketLauncherController _rocketLauncherController;
        private PlayerUpgradeHandler _upgradeHandler;

        /// <summary>
        /// (Parent) _EnterTree runs first and is good for registering and subscribing to services.
        /// </summary>
        public override void _EnterTree()
        {
            if (TargetProvider.Instance == null)
            {
                GD.PushWarning($"Player: TargetProvider.Instance is null. Player was not registered.");
            }

            TargetProvider.Instance.RegisterPlayer(this);
        }

        public override void _Ready()
        {
            _chaingunController = this.GetFirstChildOfType<PlayerChaingunController>(true);
            _railgunController = this.GetFirstChildOfType<PlayerRailgunController>(true);
            _rocketLauncherController = this.GetFirstChildOfType<PlayerRocketLauncherController>(true);
            _upgradeHandler = new PlayerUpgradeHandler();
            _upgradeHandler.RegisterWeaponController(_chaingunController);
            _upgradeHandler.RegisterWeaponController(_railgunController);
            _upgradeHandler.RegisterWeaponController(_rocketLauncherController);
        }

        /// <summary>
        /// Remove player from TargetProvider when exiting tree.
        /// </summary>
        public override void _ExitTree()
        {
            TargetProvider.Instance?.UnregisterPlayer(this);
        }

        public override void TakeDamage(DamageData damageData)
        {
            base.TakeDamage(damageData);
            GD.Print("Aaaa I'm taking damage! ");
        }
    }
}
