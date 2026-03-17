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
        [Export]
        private WeaponUiRocketLauncher _launcher1;

        [Export]
        private WeaponUiRocketLauncher _launcher2;

        [Export]
        private WeaponUiRocketLauncher _launcher3;

        [Export]
        private WeaponUiRocketLauncher _launcher4;

        private List<WeaponUiRocketLauncher> _launchers = new List<WeaponUiRocketLauncher>();

        private PlayerRocketLauncherController _launcherController;

        public override void _Ready()
        {
            GameManager.Instance.PlayerConfigurationChanged += RefreshUi;
            _launchers.Add(_launcher1);
            _launchers.Add(_launcher2);
            _launchers.Add(_launcher3);
            _launchers.Add(_launcher4);
            RefreshUi();
        }

        public override void _ExitTree()
        {
            GameManager.Instance.PlayerConfigurationChanged -= RefreshUi;
        }

        private void RefreshUi()
        {
            _launcherController = LevelManager.Active.Player.RocketLauncherController;
            ClearWeaponUiList();
            foreach (var weapon in _launcherController.Weapons)
            {
                if (weapon is RocketLauncher rocketLauncher)
                {
                    int index = _launcherController.Weapons.IndexOf(rocketLauncher);
                    _launchers[index].SetLauncher(rocketLauncher);
                    _launchers[index].IsActive = true;
                    _launchers[index].ResetIndicators();
                }
            }
            SetWeaponUiVisibility();
        }

        private void ClearWeaponUiList()
        {
            foreach (WeaponUiRocketLauncher launcherUi in _launchers)
            {
                launcherUi.ClearLauncher();
                launcherUi.IsActive = false;
            }
        }

        private void SetWeaponUiVisibility()
        {
            foreach (WeaponUiRocketLauncher launcherUi in _launchers)
            {
                launcherUi.Visible = launcherUi.IsActive;
            }
        }
    }
}
