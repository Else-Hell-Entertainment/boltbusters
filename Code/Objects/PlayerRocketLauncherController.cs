// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters
{
    public partial class PlayerRocketLauncherController : PlayerWeaponGroupController
    {
        // TODO: Implement rocket launchers adjusting to range setting. Currently not implemented!
        [Export]
        private float _range = 12f;

        private Sprite3D _reticle;

        public override void _Ready()
        {
            base._Ready();
            _reticle = GetNode<Sprite3D>("Reticle");
            _reticle.Position -= new Vector3(0, _reticle.GlobalPosition.Y - 0.2f, _range);
            AddWeapon();
            AddWeapon();
            AddWeapon();
            AddWeapon();
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
    }
}
