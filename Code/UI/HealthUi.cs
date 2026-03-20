// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class HealthUi : Control
    {
        [Export]
        private Label _healthLabel;

        private Player _player;

        public override void _Ready()
        {
            _player = LevelManager.Active.Player;
            GameManager.Instance.RequestHudRefresh += UpdateHealthLabel;
        }

        public override void _ExitTree()
        {
            GameManager.Instance.RequestHudRefresh -= UpdateHealthLabel;
        }

        private void UpdateHealthLabel()
        {
            _healthLabel.Text = LevelManager.Active.Player.GetCurrentHealth().ToString();
        }
    }
}
