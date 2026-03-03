// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class MenuShop : Menu
    {
        [Export]
        private Button _btnUpgradeChaingun;

        [Export]
        private Button _btnUpgradeRailgun;

        [Export]
        private Button _btnUpgradeRocketLauncher;

        [Export]
        private Button _btnDowngradeChaingun;

        [Export]
        private Button _btnDowngradeRailgun;

        [Export]
        private Button _btnDowngradeRocketLauncher;

        [Export]
        private Button _btnEnterNextRound;

        public override void _EnterTree()
        {
            _btnUpgradeChaingun.Pressed += OnBtnUpgradeChaingunPressed;
            _btnUpgradeRailgun.Pressed += OnBtnUpgradeRailgunPressed;
            _btnUpgradeRocketLauncher.Pressed += OnBtnUpgradeRocketLauncherPressed;
            _btnDowngradeChaingun.Pressed += OnBtnDowngradeChaingunPressed;
            _btnDowngradeRailgun.Pressed += OnBtnDowngradeRailgunPressed;
            _btnDowngradeRocketLauncher.Pressed += OnBtnDowngradeRocketLauncherPressed;
            _btnEnterNextRound.Pressed += OnBtnEnterNextRoundPressed;
        }

        public override void _ExitTree()
        {
            _btnUpgradeChaingun.Pressed -= OnBtnUpgradeChaingunPressed;
            _btnUpgradeRailgun.Pressed -= OnBtnUpgradeRailgunPressed;
            _btnUpgradeRocketLauncher.Pressed -= OnBtnUpgradeRocketLauncherPressed;
            _btnDowngradeChaingun.Pressed -= OnBtnDowngradeChaingunPressed;
            _btnDowngradeRailgun.Pressed -= OnBtnDowngradeRailgunPressed;
            _btnDowngradeRocketLauncher.Pressed -= OnBtnDowngradeRocketLauncherPressed;
            _btnEnterNextRound.Pressed -= OnBtnEnterNextRoundPressed;
        }

        private void OnBtnUpgradeChaingunPressed() { }

        private void OnBtnUpgradeRailgunPressed() { }

        private void OnBtnUpgradeRocketLauncherPressed() { }

        private void OnBtnDowngradeChaingunPressed() { }

        private void OnBtnDowngradeRailgunPressed() { }

        private void OnBtnDowngradeRocketLauncherPressed() { }

        private void OnBtnEnterNextRoundPressed() { }
    }
}
