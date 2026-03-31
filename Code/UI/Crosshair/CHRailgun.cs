// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class CHRailgun : TextureRect
    {
        [Export]
        private Color _tempGaugeGreen = Colors.Green;

        [Export]
        private Color _tempGaugeYellow = Colors.Yellow;

        [Export]
        private Color _tempGaugeOrange = Colors.Orange;

        [Export]
        private Color _tempGaugeRed = Colors.Red;

        private Railgun _railgun;

        public bool IsActive;

        public void SetRailgun(Railgun railgun)
        {
            if (_railgun != null)
            {
                _railgun.RailgunStateChanged -= OnRailgunStateChanged;
            }

            _railgun = railgun;
            _railgun.RailgunStateChanged += OnRailgunStateChanged;
            Modulate = _tempGaugeGreen;
            SetActive(true);
        }

        public void ClearRailgun()
        {
            if (_railgun != null)
            {
                _railgun.RailgunStateChanged -= OnRailgunStateChanged;
            }
            _railgun = null;
            SetActive(false);
        }

        private void OnRailgunStateChanged(int state)
        {
            Railgun.RailgunState s = (Railgun.RailgunState)state;
            switch (s)
            {
                case Railgun.RailgunState.ReadyToFire:
                    Modulate = _tempGaugeGreen;
                    break;
                case Railgun.RailgunState.Discharging:
                    Modulate = _tempGaugeOrange;
                    break;
                case Railgun.RailgunState.Charging:
                    Modulate = _tempGaugeYellow;
                    break;
                case Railgun.RailgunState.Reloading:
                    Modulate = _tempGaugeRed;
                    break;
            }
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            Visible = IsActive;
        }
    }
}
