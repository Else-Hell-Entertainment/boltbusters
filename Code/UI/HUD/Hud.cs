// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.BoltBusters.States;
using EHE.Common.Godot.Extensions;
using EHE.Common.Godot.Logging;
using Godot;
using GDCollections = Godot.Collections;

namespace EHE.BoltBusters.Ui
{
    /// <summary>
    ///  Central controller for HUD components.
    /// </summary>
    ///
    /// <remarks>
    ///  The HUD is dependent on the <see cref="GameManager.CurrentPlayerData"/>
    ///  in <see cref="GameManager"/>, and more specifically, the signals
    ///  emitted by it. These signals are used to update the different hud
    ///  components like the collectible counters. The signals are subscribed
    ///  to when the HUD node enters the scene tree and unsubscribed from when
    ///  the node exists the scene tree. If the signals cannot be connected due
    ///  to null reference, an error is logged.
    /// </remarks>
    public partial class Hud : Control
    {
        [Export]
        private AnimationPlayer _animationPlayer;

        [Export]
        private CollectibleUi _collectibleUi;

        [Export]
        private HealthUi _healthUi;

        private bool _isVisible = false;

        //private WeaponHud _weaponUi;

        /// <summary>
        ///  Connects the required signals. Logs an error if this fails.
        /// </summary>
        public override void _EnterTree()
        {
            if (GameManager.Instance == null)
            {
                this.LogError($"Cannot connect signals: {nameof(GameManager.Instance)} is null!");
                return;
            }

            GameManager.Instance.RequestHudRefresh += UpdateWeaponUi;
            GameManager.Instance.RequestHudRefreshWithPlayerData += UpdateAllUi;

            if (GameManager.Instance.CurrentPlayerData == null)
            {
                this.LogError($"Cannot connect signals: {nameof(GameManager.Instance.CurrentPlayerData)} is null!");
                return;
            }

            GameManager.Instance.CurrentPlayerData.CollectibleCountChanged += UpdateCollectibleUi;

            // Makes sure the hud is always up to date after it has entered the
            // scene. The signal responsible for hud refresh can sometimes be
            // emitted before the HUD has been loaded which causes the values
            // not to update correctly. This has been a problem mainly when
            // loading the game to the shop state from save.
            CallDeferred(nameof(UpdateAllUi), GameManager.Instance.CurrentPlayerData);

            if (GameManager.Instance.StateMachine == null)
            {
                this.LogError($"Cannot connect signals: {nameof(GameManager.Instance.StateMachine)} is null!");
                return;
            }

            GameManager.Instance.StateMachine.StateChanged += OnGameStateChanged;

            if (LevelManager.Active == null)
            {
                this.LogError($"Cannot connect signals: {nameof(LevelManager.Active)} is null!");
                return;
            }

            CallDeferred(nameof(ConnectLevelManagerSignals));
        }

        /// <summary>
        ///  Disconnects the required signals.
        /// </summary>
        public override void _ExitTree()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentPlayerData != null)
            {
                GameManager.Instance.CurrentPlayerData.CollectibleCountChanged -= UpdateCollectibleUi;
                GameManager.Instance.RequestHudRefresh -= UpdateWeaponUi;
                GameManager.Instance.RequestHudRefreshWithPlayerData -= UpdateAllUi;
            }

            if (GameManager.Instance != null && GameManager.Instance.StateMachine != null)
            {
                GameManager.Instance.StateMachine.StateChanged -= OnGameStateChanged;
            }
        }

        public override void _Ready()
        {
            _collectibleUi = this.GetFirstChildOfType<CollectibleUi>(recurse: true);
            _healthUi = this.GetFirstChildOfType<HealthUi>(recurse: true);
            //_weaponUi = this.GetFirstChildOfType<WeaponHud>();
            _animationPlayer = this.GetFirstChildOfType<AnimationPlayer>(recurse: false);

            if (_collectibleUi == null)
            {
                this.LogError("Collectible UI not assigned!");
            }

            if (_healthUi == null)
            {
                this.LogError("Health UI node not found!");
            }

            // if (_weaponUi == null)
            // {
            //     this.LogError("Weapon UI node not found!");
            // }

            if (_animationPlayer == null)
            {
                this.LogError("Animation player not assigned!");
            }
        }

        private void ConnectLevelManagerSignals()
        {
            LevelManager.Active.RoundStarted += SlideIn;
        }

        private void UpdateCollectibleUi(CollectibleType type, int value)
        {
            _collectibleUi.SetCollectibleCount(type, value);
        }

        private void UpdateCollectibleUi(int type, int value)
        {
            UpdateCollectibleUi((CollectibleType)type, value);
        }

        private void UpdateCollectibleUi(GDCollections.Dictionary<CollectibleType, int> collectibleCounts)
        {
            foreach (var (type, value) in collectibleCounts)
            {
                UpdateCollectibleUi(type, value);
            }
        }

        private void UpdateWeaponUi()
        {
            //_weaponUi.RefreshUi();
        }

        private void UpdateHealthUi()
        {
            _healthUi.UpdateHealthUi();
        }

        private void UpdateAllUi(PlayerData playerData)
        {
            UpdateCollectibleUi(playerData.GetCollectibleCounts());
            UpdateWeaponUi();
            UpdateHealthUi();
        }

        private void OnGameStateChanged(StateType nextStateType)
        {
            if (_animationPlayer == null)
            {
                this.LogError("Animation player not assigned!");
                return;
            }

            if (nextStateType == StateType.Shop)
            {
                SlideOut();
            }

            // if (!_isVisible && nextStateType == StateType.Round)
            // {
            //     SlideIn();
            // }
            // else if (_isVisible && nextStateType == StateType.Shop)
            // {
            //     SlideOut();
            // }
        }

        private void SlideIn()
        {
            if (_isVisible)
            {
                return;
            }

            _isVisible = true;
            _animationPlayer.Play("show_hud");
        }

        private void SlideOut()
        {
            if (!_isVisible)
            {
                return;
            }

            _isVisible = false;
            _animationPlayer.Play("hide_hud");
        }
    }
}
