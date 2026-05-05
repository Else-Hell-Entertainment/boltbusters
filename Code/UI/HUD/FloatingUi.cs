// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters.Ui
{
    public partial class FloatingUi : Control
    {
        [Export]
        private AnimationPlayer _animationPlayer;

        /// <inheritdoc />
        public override void _EnterTree() { }

        /// <inheritdoc />
        public override void _ExitTree()
        {
            DisconnectSignals();
        }

        /// <inheritdoc />
        public override void _Ready()
        {
            if (_animationPlayer == null)
            {
                _animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

                if (_animationPlayer == null)
                {
                    this.LogError($"No suitable node for {nameof(_animationPlayer)} found in the scene!");
                }
            }

            CallDeferred(nameof(ConnectSignals));
        }

        private void ConnectSignals()
        {
            this.LogDebug("Connecting signals.");

            if (LevelManager.Active == null || LevelManager.Active.LevelType != LevelType.Gameplay)
            {
                this.LogError("No suitable level manager found. Cannot connect signals!");
                return;
            }

            LevelManager.Active.RoundStarting += PlayCountdown;
            LevelManager.Active.RoundEnded += OnRoundEnded;
        }

        private void DisconnectSignals()
        {
            this.LogDebug("Disconnecting signals.");

            if (LevelManager.Active != null)
            {
                LevelManager.Active.RoundStarting -= PlayCountdown;
                LevelManager.Active.RoundEnded -= OnRoundEnded;
            }
        }

        private void PlayCountdown()
        {
            _animationPlayer.Play("round_starting");
        }

        private void OnRoundEnded()
        {
            _animationPlayer.Play("round_ended");
        }
    }
}
