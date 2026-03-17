// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters.Ui
{
    /// <summary>
    /// Controls all weapon UI elements shown on the HUD (rockets, railguns and chaingun). All weapon groups have their
    /// own internal logic, this element acts as top level manager to assign correct weapons to their UI elements.
    /// </summary>
    public partial class WeaponHud : Control
    {
        [ExportGroup("Weapon UI nodes")]
        [Export]
        private WeaponUiRocketLauncher _launcher1;

        [Export]
        private WeaponUiRocketLauncher _launcher2;

        [Export]
        private WeaponUiRocketLauncher _launcher3;

        [Export]
        private WeaponUiRocketLauncher _launcher4;

        [Export]
        private WeaponUiRailgun _railgun1;

        [Export]
        private WeaponUiRailgun _railgun2;

        [Export]
        private WeaponUiRailgun _railgun3;

        [Export]
        private WeaponUiRailgun _railgun4;

        [Export]
        private WeaponUiChaingun _chaingunUi;

        private List<WeaponUiRocketLauncher> _launchers = new List<WeaponUiRocketLauncher>();
        private List<WeaponUiRailgun> _railguns = new List<WeaponUiRailgun>();

        public override void _Ready()
        {
            GameManager.Instance.PlayerConfigurationChanged += RefreshUi;
            _launchers.Add(_launcher1);
            _launchers.Add(_launcher2);
            _launchers.Add(_launcher3);
            _launchers.Add(_launcher4);
            _railguns.Add(_railgun1);
            _railguns.Add(_railgun2);
            _railguns.Add(_railgun3);
            _railguns.Add(_railgun4);
            CallDeferred(MethodName.RefreshUi);
        }

        public override void _ExitTree()
        {
            GameManager.Instance.PlayerConfigurationChanged -= RefreshUi;
        }

        private void RefreshUi()
        {
            ClearWeaponUiList();
            UpdateLauncherList();
            UpdateRailgunList();
            UpdateChaingun();
            SetWeaponUiVisibility();
        }

        private void UpdateChaingun()
        {
            _chaingunUi.SetChaingunController(LevelManager.Active.Player.ChaingunController);
        }

        private void UpdateLauncherList()
        {
            foreach (var weapon in LevelManager.Active.Player.RocketLauncherController.Weapons)
            {
                if (weapon is RocketLauncher rocketLauncher)
                {
                    int index = LevelManager.Active.Player.RocketLauncherController.Weapons.IndexOf(rocketLauncher);
                    _launchers[index].SetLauncher(rocketLauncher);
                    _launchers[index].IsActive = true;
                    _launchers[index].ResetIndicators();
                }
            }
        }

        private void UpdateRailgunList()
        {
            foreach (var weapon in LevelManager.Active.Player.RailgunController.Weapons)
            {
                if (weapon is Railgun railgun)
                {
                    int index = LevelManager.Active.Player.RailgunController.Weapons.IndexOf(railgun);
                    _railguns[index].SetRailgun(railgun);
                    _railguns[index].IsActive = true;
                    _railguns[index].ResetIndicators();
                }
            }
        }

        private void ClearWeaponUiList()
        {
            foreach (WeaponUiRocketLauncher launcherUi in _launchers)
            {
                launcherUi.ClearLauncher();
                launcherUi.IsActive = false;
            }

            foreach (WeaponUiRailgun railgunUi in _railguns)
            {
                railgunUi.IsActive = false;
                railgunUi.ClearRailgun();
            }
        }

        private void SetWeaponUiVisibility()
        {
            foreach (WeaponUiRocketLauncher launcherUi in _launchers)
            {
                launcherUi.Visible = launcherUi.IsActive;
            }

            foreach (WeaponUiRailgun railgunUi in _railguns)
            {
                railgunUi.Visible = railgunUi.IsActive;
            }
        }
    }
}
