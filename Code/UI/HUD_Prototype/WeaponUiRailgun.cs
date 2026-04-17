// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class WeaponUiRailgun : Control
    {
        private Railgun _railgun;

        [Export]
        private TextureRect _readyTexture;

        [Export]
        private TextureRect _reloadingTexture;

        private Color _colorGreen = Colors.LimeGreen;
        private Color _colorRed = Colors.Red;
        private Color _colorGray = Colors.Gray;

        public bool IsActive;

        public void SetRailgun(Railgun railgun)
        {
            DisconnectSignals();
            _railgun = railgun;
            ConnectSignals();
        }

        public void ClearRailgun()
        {
            DisconnectSignals();
            _railgun = null;
        }

        public void ResetIndicators()
        {
            ToggleReadyIndicator(true);
            ToggleReloadingIndicator(false);
        }

        public override void _ExitTree()
        {
            DisconnectSignals();
        }

        private void ConnectSignals()
        {
            _railgun.RailgunStateChanged += OnRailgunStateChanged;
        }

        private void DisconnectSignals()
        {
            if (_railgun != null)
            {
                _railgun.RailgunStateChanged -= OnRailgunStateChanged;
            }
        }

        private void OnRailgunStateChanged(int state)
        {
            Railgun.RailgunState railgunState = (Railgun.RailgunState)state;

            switch (railgunState)
            {
                case Railgun.RailgunState.ReadyToFire:
                    ToggleReadyIndicator(true);
                    break;
                case Railgun.RailgunState.Reloading:
                    break;
                case Railgun.RailgunState.Charging:
                    break;
                case Railgun.RailgunState.Discharging:
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
