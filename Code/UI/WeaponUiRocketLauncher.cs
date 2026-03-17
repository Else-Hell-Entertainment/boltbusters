// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using System;
using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class WeaponUiRocketLauncher : Control
    {
        private RocketLauncher _launcher;

        [Export]
        private TextureRect _readyTexture;

        [Export]
        private TextureRect _launchingTexture;

        [Export]
        private TextureRect _reloadingTexture;

        private Color _colorGreen = Colors.LimeGreen;
        private Color _colorYellow = Colors.Yellow;
        private Color _colorRed = Colors.Red;
        private Color _colorGray = Colors.Gray;

        private float _launchBlinkerDuration = 0.1f;

        public bool IsActive;

        public void SetLauncher(RocketLauncher rocketLauncher)
        {
            DisconnectSignals();
            _launcher = rocketLauncher;
            ConnectSignals();
        }

        public void ResetIndicators()
        {
            ToggleReadyIndicator(true);
            ToggleReloadingIndicator(false);
            _launchingTexture.Modulate = _colorGray;
        }

        public void ClearLauncher()
        {
            DisconnectSignals();
            _launcher = null;
        }

        public override void _ExitTree()
        {
            DisconnectSignals();
        }

        private void ConnectSignals()
        {
            _launcher.RocketLauncherStateChanged += OnLauncherStateChanged;
        }

        private void DisconnectSignals()
        {
            if (_launcher != null)
            {
                _launcher.RocketLauncherStateChanged -= OnLauncherStateChanged;
            }
        }

        private void OnLauncherStateChanged(int state)
        {
            RocketLauncher.LauncherState launcherState = (RocketLauncher.LauncherState)state;
            switch (launcherState)
            {
                case RocketLauncher.LauncherState.ReadyToFire:
                    ToggleReadyIndicator(true);
                    break;
                case RocketLauncher.LauncherState.NotReadyToFire:
                    ToggleReadyIndicator(false);
                    break;
                case RocketLauncher.LauncherState.RocketJustLaunched:
                    BlinkLaunchingIndicator(_launchBlinkerDuration);
                    break;
                case RocketLauncher.LauncherState.ReloadingStarted:
                    ToggleReloadingIndicator(true);
                    break;
                case RocketLauncher.LauncherState.ReloadingFinished:
                    ToggleReloadingIndicator(false);
                    break;
            }
        }

        private void ToggleReadyIndicator(bool activeStatus)
        {
            if (activeStatus)
            {
                _readyTexture.Modulate = _colorGreen;
            }
            else
            {
                _readyTexture.Modulate = _colorGray;
            }
        }

        private void BlinkLaunchingIndicator(float duration)
        {
            Tween blinkTween = CreateTween();
            blinkTween.TweenProperty(_launchingTexture, "modulate", _colorYellow, 0.01f);
            blinkTween.TweenInterval(duration);
            blinkTween.TweenProperty(_launchingTexture, "modulate", _colorGray, 0.01f);
        }

        private void ToggleReloadingIndicator(bool activeStatus)
        {
            if (activeStatus)
            {
                _reloadingTexture.Modulate = _colorRed;
            }
            else
            {
                _reloadingTexture.Modulate = _colorGray;
            }
        }
    }
}
