using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    public partial class PlayerRailgunController : PlayerWeaponGroupController
    {
        private Railgun activeRailgun;

        private List<Railgun> _connectedRailguns = new List<Railgun>();

        public override void _Ready()
        {
            base._Ready();
            AddWeapon();
            AddWeapon();
            AddWeapon();
            AddWeapon();
        }

        public override void AddWeapon()
        {
            base.AddWeapon();
            RefreshConnectedRailguns();
            SetNextActive();
        }

        public override void Attack()
        {
            if (activeRailgun == null)
            {
                return;
            }
            activeRailgun.Attack();
            activeRailgun.IsActive = false;
            activeRailgun = null;
            SetNextActive();
        }

        private void RefreshConnectedRailguns()
        {
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon is Railgun railgun)
                {
                    if (!_connectedRailguns.Contains(railgun))
                    {
                        railgun.RailgunReloadReady += OnRailgunReloadReady;
                        _connectedRailguns.Add(railgun);
                    }
                }
            }

            foreach (Railgun railgun in _connectedRailguns)
            {
                if (!Weapons.Contains(railgun))
                {
                    railgun.RailgunReloadReady -= OnRailgunReloadReady;
                    _connectedRailguns.Remove(railgun);
                }
            }
        }

        private void OnRailgunReloadReady(Railgun railgun)
        {
            if (activeRailgun == null)
            {
                activeRailgun = railgun;
                activeRailgun.IsActive = true;
            }
        }

        private void SetNextActive()
        {
            foreach (BaseWeapon weapon in Weapons)
            {
                if (activeRailgun == null && weapon.CanAttack() && weapon is Railgun railgun)
                {
                    railgun.IsActive = true;
                    activeRailgun = railgun;
                    return;
                }
            }
        }
    }
}
