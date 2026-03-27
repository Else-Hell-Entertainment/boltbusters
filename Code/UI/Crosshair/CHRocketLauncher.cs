using System;
using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class CHRocketLauncher : TextureRect
    {
        public bool IsActive;

        [Export]
        private Color _tempGaugeGreen = Colors.Green;

        [Export]
        private Color _tempGaugeYellow = Colors.Yellow;

        [Export]
        private Color _tempGaugeOrange = Colors.Orange;

        [Export]
        private Color _tempGaugeRed = Colors.Red;

        private RocketLauncher _launcher;

        public void SetLauncher(RocketLauncher launcher)
        {
            if (_launcher != null)
            {
                _launcher.RocketLauncherStateChanged -= OnLauncherStateChanged;
            }

            _launcher = launcher;
            _launcher.RocketLauncherStateChanged += OnLauncherStateChanged;
            Modulate = _tempGaugeGreen;
            SetActive(true);
        }

        public void ClearLauncher()
        {
            if (_launcher != null)
            {
                _launcher.RocketLauncherStateChanged -= OnLauncherStateChanged;
            }
            _launcher = null;
            SetActive(false);
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            if (IsActive)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void OnLauncherStateChanged(int state)
        {
            RocketLauncher.LauncherState s = (RocketLauncher.LauncherState)state;
            switch (s)
            {
                case RocketLauncher.LauncherState.ReadyToFire:
                    Modulate = _tempGaugeGreen;
                    break;
                case RocketLauncher.LauncherState.LaunchingRockets:
                    Modulate = _tempGaugeYellow;
                    break;
                case RocketLauncher.LauncherState.RocketJustLaunched:
                    GD.Print(_launcher + "Launched rockets");
                    break;
                case RocketLauncher.LauncherState.NotReadyToFire:
                    Modulate = _tempGaugeOrange;
                    break;
                case RocketLauncher.LauncherState.ReloadingStarted:
                    GD.Print(_launcher + "Reloading started");
                    break;
                case RocketLauncher.LauncherState.ReloadingFinished:
                    GD.Print(_launcher + "Reloading finished");
                    break;
            }
        }
    }
}
