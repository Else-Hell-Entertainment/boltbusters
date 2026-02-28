// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>
//            Miska Rihu <miska.rihu@tuni.fi>

using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    public partial class PlayerRailgunController : PlayerWeaponGroupController
    {
        private Railgun _activeRailgun;

        private List<Railgun> _connectedRailguns = new List<Railgun>();

        public override WeaponType WeaponType => WeaponType.Railgun;

        public override void _Ready()
        {
            base._Ready();
            AddWeapon();
            AddWeapon();
            AddWeapon();
            AddWeapon();
        }

        public override bool AddWeapon()
        {
            if (!base.AddWeapon())
            {
                return false;
            }
            RefreshConnectedRailguns();
            SetNextActive();
            return true;
        }

        public override bool RemoveWeapon()
        {
            if (!base.RemoveWeapon())
            {
                return false;
            }
            RefreshConnectedRailguns();
            SetNextActive();
            return true;
        }

        public override void Attack()
        {
            if (_activeRailgun == null)
            {
                return;
            }
            _activeRailgun.Attack();
            _activeRailgun.IsActive = false;
            _activeRailgun = null;
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

            List<Railgun> removeList = new List<Railgun>();
            foreach (Railgun railgun in _connectedRailguns)
            {
                if (!Weapons.Contains(railgun))
                {
                    railgun.RailgunReloadReady -= OnRailgunReloadReady;
                    removeList.Add(railgun);
                }
            }

            foreach (Railgun railgun in removeList)
            {
                _connectedRailguns.Remove(railgun);
            }
        }

        private void OnRailgunReloadReady(Railgun railgun)
        {
            if (_activeRailgun == null)
            {
                _activeRailgun = railgun;
                _activeRailgun.IsActive = true;
            }
        }

        private void SetNextActive()
        {
            foreach (BaseWeapon weapon in Weapons)
            {
                if (_activeRailgun == null && weapon.CanAttack && weapon is Railgun railgun)
                {
                    railgun.IsActive = true;
                    _activeRailgun = railgun;
                    return;
                }
            }
        }
    }
}
