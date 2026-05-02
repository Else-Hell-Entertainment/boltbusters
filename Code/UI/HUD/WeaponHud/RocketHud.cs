// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class RocketHud : Control
    {
        private PlayerRocketLauncherController _launcherController;

        private TextureRect[] _launcherIcons = new TextureRect[4];

        [ExportGroup("Node references")]
        [Export]
        private TextureRect _launcherIcon1;

        [Export]
        private TextureRect _launcherIcon2;

        [Export]
        private TextureRect _launcherIcon3;

        [Export]
        private TextureRect _launcherIcon4;

        public override void _Ready()
        {
            base._Ready();
            // With only 4 icons this is hardcoded for now (won't change).
            _launcherIcons[0] = _launcherIcon1;
            _launcherIcons[1] = _launcherIcon2;
            _launcherIcons[2] = _launcherIcon3;
            _launcherIcons[3] = _launcherIcon4;

            CallDeferred(MethodName.Initialize);
        }

        private void Initialize()
        {
            if (_launcherController != null)
            {
                ClearLauncherSignals();
                _launcherController.RocketLauncherConfigurationChanged -= OnConfigurationChanged;
            }

            _launcherController = LevelManager.Active.Player.RocketLauncherController;
            _launcherController.RocketLauncherConfigurationChanged += OnConfigurationChanged;
            ConnectLauncherSignals();
            RefreshIcons();
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            if (_launcherController != null)
            {
                ClearLauncherSignals();
                _launcherController.RocketLauncherConfigurationChanged -= OnConfigurationChanged;
            }
        }

        private void OnConfigurationChanged()
        {
            ClearLauncherSignals();
            ConnectLauncherSignals();
            RefreshIcons();
        }

        private void ConnectLauncherSignals()
        {
            for (int i = 0; i < _launcherController.Weapons.Count; i++)
            {
                BaseWeapon wp = _launcherController.Weapons[i];
                if (wp is RocketLauncher rl)
                {
                    rl.RocketLauncherStateChanged += OnLauncherStateChanged;
                }
            }
        }

        private void ClearLauncherSignals()
        {
            for (int i = 0; i < _launcherController.Weapons.Count; i++)
            {
                BaseWeapon wp = _launcherController.Weapons[i];
                if (wp is RocketLauncher rl)
                {
                    rl.RocketLauncherStateChanged -= OnLauncherStateChanged;
                }
            }
        }

        private void OnLauncherStateChanged(int state)
        {
            RefreshIcons();
        }

        private void RefreshIcons()
        {
            int launcherCount = _launcherController.Weapons.Count;

            for (int i = 0; i < _launcherIcons.Length; i++)
            {
                if (i < launcherCount)
                {
                    _launcherIcons[i].Show();
                }
                else
                {
                    _launcherIcons[i].Hide();
                }
            }

            int reloadingCount = 0;
            int readyToFireCount = 0;

            foreach (BaseWeapon wp in _launcherController.Weapons)
            {
                if (wp.CanAttack)
                {
                    readyToFireCount++;
                }
                else
                {
                    reloadingCount++;
                }
            }

            for (int i = 0; i < readyToFireCount; i++)
            {
                SetIconStateReadyToFire(i);
            }

            for (int i = readyToFireCount; i < readyToFireCount + reloadingCount; i++)
            {
                SetIconStateCharging(i);
            }
        }

        private void SetIconStateCharging(int iconIndex)
        {
            _launcherIcons[iconIndex].Modulate = new Color(1, 1, 1, 0.2f);
        }

        private void SetIconStateReadyToFire(int iconIndex)
        {
            _launcherIcons[iconIndex].Modulate = new Color(1, 1, 1, 1f);
        }
    }
}
