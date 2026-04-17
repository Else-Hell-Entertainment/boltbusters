// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class ChaingunHud : Control
    {
        [ExportGroup("Color Definitions")]
        [Export]
        private Color _green = Colors.Green;

        [Export]
        private Color _yellow = Colors.Yellow;

        [Export]
        private Color _red = Colors.Red;

        [ExportGroup("Node references")]
        [Export]
        private TextureProgressBar _progressBar;

        private PlayerChaingunController _playerChaingunController;

        private bool _isOverHeating;

        private float _warningThreshold = 80f;
        private float _overheatLimit = 100f;

        public override void _Ready()
        {
            base._Ready();
            GameManager.Instance.RequestHudRefresh += RefreshHud;
            CallDeferred(MethodName.RefreshHud);
        }

        public override void _EnterTree()
        {
            base._EnterTree();
            RefreshHud();
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            DisconnectSignals();
        }

        private void RefreshHud()
        {
            DisconnectSignals();
            _playerChaingunController = LevelManager.Active.Player.ChaingunController;
            ConnectSignals();
        }

        private void OnChaingunStateChanged(int state)
        {
            PlayerChaingunController.ChaingunState chaingunState = (PlayerChaingunController.ChaingunState)state;
            switch (chaingunState)
            {
                case PlayerChaingunController.ChaingunState.Firing:
                    break;
                case PlayerChaingunController.ChaingunState.Overheat:
                    ToggleOverheat(true);
                    break;
                case PlayerChaingunController.ChaingunState.HeatChanged:
                    UpdateHeat();
                    break;
                case PlayerChaingunController.ChaingunState.NotReadyToFire:
                    break;
                case PlayerChaingunController.ChaingunState.ReadyToFire:
                    break;
                case PlayerChaingunController.ChaingunState.BarrelCountChanged:
                    break;
            }
        }

        private void ToggleOverheat(bool isOverheat)
        {
            if (isOverheat)
            {
                _isOverHeating = true;
                _progressBar.TintProgress = _red;
            }
            else
            {
                _isOverHeating = false;
            }
        }

        private void UpdateHeat()
        {
            float _heatValue = _playerChaingunController.GetCurrentHeat();
            _progressBar.Value = _heatValue;
            if (_heatValue < _warningThreshold)
            {
                _progressBar.TintProgress = _green;
                _isOverHeating = false;
            }

            if (!_isOverHeating && _heatValue >= _warningThreshold)
            {
                _progressBar.TintProgress = _yellow;
            }
        }

        private void DisconnectSignals()
        {
            if (_playerChaingunController != null)
            {
                _playerChaingunController.ChaingunStateChanged -= OnChaingunStateChanged;
            }
            GameManager.Instance.RequestHudRefresh -= RefreshHud;
        }

        private void ConnectSignals()
        {
            if (_playerChaingunController != null)
            {
                _playerChaingunController.ChaingunStateChanged += OnChaingunStateChanged;
            }
        }
    }
}
