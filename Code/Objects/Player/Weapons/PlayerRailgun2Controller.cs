using System;
using Godot;

namespace EHE.BoltBusters
{
    public partial class PlayerRailgun2Controller : PlayerWeaponGroupController
    {
        public override WeaponType WeaponType => WeaponType.Railgun;

        private Railgun2 _activeRailgun;

        public override void _Ready()
        {
            base._Ready();
            AddWeapon();
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
        }

        public override void Attack()
        {
            if (_activeRailgun == null)
            {
                _activeRailgun = GetNextActiveRailgun();
                if (_activeRailgun == null)
                {
                    return;
                }
            }
            _activeRailgun.Attack();
            if (_activeRailgun.ChargeReady)
            {
                _activeRailgun.Discharge();
                ShootRailgun();
                GetNextActiveRailgun();
            }
        }

        private void ShootRailgun()
        {
            GD.Print("RAILGUN GOES KEKEKEKEKEKKEKEKEKE");
        }

        private Railgun2 GetNextActiveRailgun()
        {
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon is Railgun2 railgun && railgun.CurrentState == Railgun2.RailgunState.ReadyToFire)
                {
                    return railgun;
                }
            }
            return null;
        }
    }
}
