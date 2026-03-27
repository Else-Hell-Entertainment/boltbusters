// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class Crosshair : Control
    {
        [Export]
        private CHTempGauge _tempGauge;

        private PlayerChaingunController _chaingunController;

        private PlayerRocketLauncherController _rocketLauncherController;

        private PlayerRailgunController _railgunController;

        private List<RocketLauncher> _launcherList = new List<RocketLauncher>();

        private List<Railgun> _railgunList = new List<Railgun>();

        [Export]
        private CHRocketLauncher _launcherUi0;

        [Export]
        private CHRocketLauncher _launcherUi1;

        [Export]
        private CHRocketLauncher _launcherUi2;

        [Export]
        private CHRocketLauncher _launcherUi3;
        private CHRocketLauncher[] _launcherUiArray = new CHRocketLauncher[4];

        [Export]
        private CHRailgun _railgunUi0;

        [Export]
        private CHRailgun _railgunUi1;

        [Export]
        private CHRailgun _railgunUi2;

        [Export]
        private CHRailgun _railgunUi3;
        private CHRailgun[] _railgunUiArray = new CHRailgun[4];

        public override void _Ready()
        {
            _launcherUiArray[0] = _launcherUi0;
            _launcherUiArray[1] = _launcherUi1;
            _launcherUiArray[2] = _launcherUi2;
            _launcherUiArray[3] = _launcherUi3;

            _railgunUiArray[0] = _railgunUi0;
            _railgunUiArray[1] = _railgunUi1;
            _railgunUiArray[2] = _railgunUi2;
            _railgunUiArray[3] = _railgunUi3;

            CallDeferred(MethodName.Initialize);
        }

        private void Initialize()
        {
            _chaingunController = LevelManager.Active.Player.ChaingunController;
            _chaingunController.ChaingunStateChanged += OnChaingunStateChanged;
            _rocketLauncherController = LevelManager.Active.Player.RocketLauncherController;
            _rocketLauncherController.RocketLauncherConfigurationChanged += RefreshLauncherList;
            _railgunController = LevelManager.Active.Player.RailgunController;
            _railgunController.RailgunConfigurationChanged += RefreshRailgunList;
            RefreshLauncherList();
        }

        private void OnChaingunStateChanged(int state)
        {
            PlayerChaingunController.ChaingunState chaingunState = (PlayerChaingunController.ChaingunState)state;
            switch (chaingunState)
            {
                case PlayerChaingunController.ChaingunState.Firing:
                    break;
                case PlayerChaingunController.ChaingunState.HeatChanged:
                    _tempGauge.SetGaugeFill(_chaingunController.GetCurrentHeat());
                    break;
                case PlayerChaingunController.ChaingunState.Overheat:
                    _tempGauge.IsOverheating = true;
                    break;
                case PlayerChaingunController.ChaingunState.ReadyToFire:
                    _tempGauge.IsOverheating = false;
                    break;
                case PlayerChaingunController.ChaingunState.BarrelCountChanged:
                    break;
            }
        }

        private void RefreshLauncherList()
        {
            foreach (CHRocketLauncher ch in _launcherUiArray)
            {
                ch.IsActive = false;
                ch.ClearLauncher();
            }

            _launcherList.Clear();
            foreach (BaseWeapon weapon in LevelManager.Active.Player.RocketLauncherController.Weapons)
            {
                if (weapon is RocketLauncher rl)
                {
                    _launcherList.Add(rl);
                }
            }

            foreach (RocketLauncher launcher in _launcherList)
            {
                GD.Print(launcher.Name);
                // TODO: VERY SCARY! CAN BREAK MUCH! FIX!
                int index = _launcherList.IndexOf(launcher);
                _launcherUiArray[index].SetLauncher(launcher);
            }
        }

        private void RefreshRailgunList()
        {
            foreach (CHRailgun ch in _railgunUiArray)
            {
                ch.IsActive = false;
                ch.ClearRailgun();
            }

            _railgunList.Clear();
            foreach (BaseWeapon weapon in LevelManager.Active.Player.RailgunController.Weapons)
            {
                if (weapon is Railgun rg)
                {
                    _railgunList.Add(rg);
                }
            }

            foreach (Railgun railgun in _railgunList)
            {
                // TODO: VERY SCARY! CAN BREAK MUCH! FIX!
                int index = _railgunList.IndexOf(railgun);
                _railgunUiArray[index].SetRailgun(railgun);
            }
        }
    }
}
