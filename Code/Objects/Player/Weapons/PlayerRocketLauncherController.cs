// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>
//            Miska Rihu <miska.rihu@tuni.fi>

using Godot;

namespace EHE.BoltBusters
{
    public partial class PlayerRocketLauncherController : PlayerWeaponGroupController
    {
        // TODO: Implement rocket launchers adjusting to range setting. Currently not implemented!
        [Export]
        private float _range = 12f;

        private Sprite3D _reticle;

        public override WeaponType WeaponType => WeaponType.Rocket;

        /// <summary>
        /// Counter for how many salvo size upgrades have been bought. Use Upgrade/DowngradeSalvoSize to change.
        /// </summary>
        public int SalvoSizeUpgradeCount { get; private set; }

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

        public void UpgradeSalvoSize()
        {
            SalvoSizeUpgradeCount++;
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon is RocketLauncher launcher)
                {
                    launcher.IncreaseSalvoSize();
#if Debug
                    GD.Print("Increasing rocket launcher salvo size");
#endif
                }
            }
        }

        public void DowngradeSalvoSize()
        {
            SalvoSizeUpgradeCount--;
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon is RocketLauncher launcher)
                {
                    launcher.DecreaseSalvoSize();
#if Debug
                    GD.Print("Decreasing rocket launcher salvo size");
#endif
                }
            }
        }
    }
}
