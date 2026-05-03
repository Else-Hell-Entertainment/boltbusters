// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class RoundIndexUi : Control
    {
        [Export]
        private Label _valueLabel;

        private PlayerData _playerData;

        public override void _EnterTree()
        {
            // Cache reference to current player data.
            _playerData = GameManager.Instance.CurrentPlayerData;

            // Connect level index changed signal from current player data.
            if (_playerData != null)
            {
                _playerData.LevelIndexChanged += SetIndex;
            }
        }

        public override void _ExitTree()
        {
            // Disconnect level index changed signal from current player data.
            if (_playerData != null)
            {
                _playerData.LevelIndexChanged -= SetIndex;
            }

            _playerData = null;
        }

        public override void _Ready()
        {
            // Check validity of node references.
            if (_valueLabel == null)
            {
                this.LogError($"Value label not assigned!");
                return;
            }

            // Fetch initial value for the index.
            if (_playerData != null)
            {
                SetIndex(_playerData.LevelIndex);
            }
        }

        /// <summary>
        ///  Updates the round index value label.
        /// </summary>
        /// <param name="newLevelIndex"></param>
        public void SetIndex(int newLevelIndex)
        {
            if (_valueLabel == null)
            {
                this.LogError("Value label not assigned!");
                return;
            }

            _valueLabel.Text = $"{newLevelIndex}";
        }
    }
}
