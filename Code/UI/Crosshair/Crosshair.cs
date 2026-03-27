// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using System.ComponentModel;
using Godot;

namespace EHE.BoltBusters
{
    public partial class Crosshair : Control
    {
        [Export]
        private CHTempGauge _tempGauge;

        private PlayerChaingunController _chaingunController;

        public override void _Ready()
        {
            CallDeferred(MethodName.Initialize);
        }

        private void Initialize()
        {
            _chaingunController = LevelManager.Active.Player.ChaingunController;
            _chaingunController.ChaingunStateChanged += OnChaingunStateChanged;
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
    }
}
