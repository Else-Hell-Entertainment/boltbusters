// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class WeaponUiChaingun : Control
    {
        [Export]
        private TextureRect _readyTexture;

        [Export]
        private TextureRect _overheatTexture;

        [Export]
        private Label _temperatureLabel;

        [Export]
        private Label _barrelCountLabel;

        private int _barrelCount = 0;

        private PlayerChaingunController _chaingunController;

        private Color _colorGreen = Colors.LimeGreen;
        private Color _colorRed = Colors.Red;
        private Color _colorGray = Colors.Gray;

        public void SetChaingunController(PlayerChaingunController chaingunController)
        {
            DisconnectSignals();
            _chaingunController = chaingunController;
            ConnectSignals();
        }

        public override void _ExitTree()
        {
            DisconnectSignals();
        }

        private void ConnectSignals()
        {
            _chaingunController.ChaingunStateChanged += OnChaingunStateChanged;
        }

        private void DisconnectSignals()
        {
            if (_chaingunController != null)
            {
                _chaingunController.ChaingunStateChanged -= OnChaingunStateChanged;
            }
        }

        private void OnChaingunStateChanged(int state)
        {
            PlayerChaingunController.ChaingunState chaingunState = (PlayerChaingunController.ChaingunState)state;
            switch (chaingunState)
            {
                case PlayerChaingunController.ChaingunState.Firing:
                    BlinkFireIndicator();
                    break;
                case PlayerChaingunController.ChaingunState.Overheat:
                    ToggleOverheat(true);
                    break;
                case PlayerChaingunController.ChaingunState.HeatChanged:
                    UpdateHeat();
                    break;
                case PlayerChaingunController.ChaingunState.NotReadyToFire:
                    ToggleFireIndicator(false);
                    break;
                case PlayerChaingunController.ChaingunState.ReadyToFire:
                    ToggleFireIndicator(true);
                    break;
            }
        }

        private void BlinkFireIndicator()
        {
            Tween blinkTween = CreateTween();
            blinkTween.TweenProperty(_readyTexture, "modulate", _colorGreen, 0.01f);
            blinkTween.TweenInterval(0.01f);
            blinkTween.TweenProperty(_readyTexture, "modulate", _colorGray, 0.01f);
        }

        private void ToggleFireIndicator(bool isReady)
        {
            if (isReady)
            {
                _readyTexture.Modulate = _colorGreen;
            }
            else
            {
                _readyTexture.Modulate = _colorRed;
            }
        }

        private void UpdateHeat()
        {
            _temperatureLabel.Text = _chaingunController.GetCurrentHeat().ToString();
        }

        private void ToggleOverheat(bool isOverheating)
        {
            if (isOverheating)
            {
                _overheatTexture.Modulate = _colorRed;
            }
            else
            {
                _overheatTexture.Modulate = _colorGray;
            }
        }
    }
}
