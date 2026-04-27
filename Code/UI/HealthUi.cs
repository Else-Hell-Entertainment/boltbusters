// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class HealthUi : Control
    {
        [Export]
        private Color _green = Colors.Green;

        [Export]
        private Color _yellow = Colors.Yellow;

        [Export]
        private Color _red = Colors.Red;

        [Export]
        private TextureProgressBar _healthBar;

        [Export]
        private int _greenThreshold = 50;

        [Export]
        private int _yellowThreshold = 25;

        public override void _Ready()
        {
            GameManager.Instance.RequestHudRefresh += UpdateHealthUi;
            UpdateHealthUi();
        }

        public override void _ExitTree()
        {
            GameManager.Instance.RequestHudRefresh -= UpdateHealthUi;
        }

        public void UpdateHealthUi()
        {
            float health = LevelManager.Active.Player.GetCurrentHealth();
            _healthBar.Value = health;

            if (health >= _greenThreshold)
            {
                _healthBar.TintProgress = _green;
            }
            else if (health >= _yellowThreshold)
            {
                _healthBar.TintProgress = _yellow;
            }
            else
            {
                _healthBar.TintProgress = _red;
            }
        }
    }
}
