// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>
//            Miska Rihu <miska.rihu@tuni.fi>

using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters
{
    public partial class PlayerRocketLauncherController : PlayerWeaponGroupController
    {
        // TODO: Implement rocket launchers adjusting to range setting. Currently not implemented!
        [Export]
        private float _range = 12f;

        [Export]
        private int _maxSecondaryUpgradeCount = 4;

        private Sprite3D _reticle;

        public override WeaponType WeaponType => WeaponType.Rocket;

        [Signal]
        public delegate void RocketLauncherConfigurationChangedEventHandler();

        public override void _Ready()
        {
            base._Ready();
            _reticle = GetNode<Sprite3D>("Reticle");
            _reticle.Position -= new Vector3(0, _reticle.GlobalPosition.Y - 0.2f, _range);
        }

        public override void Attack()
        {
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon.CanAttack)
                {
                    weapon.Attack();
                    return;
                }
            }
        }

        public override bool AddWeapon()
        {
            if (base.AddWeapon())
            {
                EmitSignal(SignalName.RocketLauncherConfigurationChanged);
                return true;
            }

            return false;
        }

        public override bool RemoveWeapon()
        {
            if (base.RemoveWeapon())
            {
                EmitSignal(SignalName.RocketLauncherConfigurationChanged);
                return true;
            }

            return false;
        }

        public bool UpgradeSalvoSize()
        {
            if (SecondaryUpgradeCount >= _maxSecondaryUpgradeCount)
            {
                return false;
            }

            SecondaryUpgradeCount++;

            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon is RocketLauncher launcher)
                {
                    launcher.IncreaseSalvoSize();
                    this.LogDebug("Increasing rocket launcher salvo size");
                }
            }

            EmitSignal(SignalName.RocketLauncherConfigurationChanged);
            return true;
        }

        public bool DowngradeSalvoSize()
        {
            if (SecondaryUpgradeCount <= 0)
            {
                return false;
            }

            SecondaryUpgradeCount--;

            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon is RocketLauncher launcher)
                {
                    launcher.DecreaseSalvoSize();
                    this.LogDebug("Decreasing rocket launcher salvo size");
                }
            }

            EmitSignal(SignalName.RocketLauncherConfigurationChanged);
            return true;
        }

        /// <summary>
        ///  Upgrades the rocket launcher. <see cref="UpgradeType.Primary"/>
        ///  adds more weapons to this controller.
        ///  <see cref="UpgradeType.Secondary"/> upgrades the salvo size.
        /// </summary>
        ///
        /// <param name="type"><inheritdoc/></param>
        protected override bool OnUpgrade(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Primary:
                    return AddWeapon();
                case UpgradeType.Secondary:
                    return UpgradeSalvoSize();
                default:
                    this.LogWarning("Unknown upgrade type.");
                    break;
            }

            return false;
        }

        /// <summary>
        ///  Downgrades the rocket launcher. <see cref="UpgradeType.Primary"/>
        ///  removes weapons from this controller.
        ///  <see cref="UpgradeType.Secondary"/> downgrades the salvo size.
        /// </summary>
        ///
        /// <param name="type"><inheritdoc/></param>
        protected override bool OnDowngrade(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Primary:
                    return RemoveWeapon();
                case UpgradeType.Secondary:
                    return DowngradeSalvoSize();
                default:
                    this.LogWarning("Unknown upgrade type.");
                    break;
            }

            return false;
        }
    }
}
