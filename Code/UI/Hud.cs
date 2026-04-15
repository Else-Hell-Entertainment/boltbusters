// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.Common.Godot.Extensions;
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
        private CollectibleUi _collectibleUi;

        //private WeaponHud _weaponUi;

        /// <summary>
        ///  Connects the required signals. Logs an error if this fails.
        /// </summary>
        public override void _EnterTree()
        {
            if (GameManager.Instance == null)
            {
                GD.PushError("GameManager instance not found!");
                return;
            }

            if (GameManager.Instance.CurrentPlayerData == null)
            {
                GD.PushError($"Cannot connect signals. {nameof(GameManager.Instance.CurrentPlayerData)} is null!");
                return;
            }

            GameManager.Instance.CurrentPlayerData.CollectibleCountChanged += UpdateCollectibleUi;
            GameManager.Instance.RequestHudRefresh += UpdateWeaponUi;
            GameManager.Instance.RequestHudRefreshWithPlayerData += UpdateAllUi;
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
        }

        public override void _Ready()
        {
            _collectibleUi = this.GetFirstChildOfType<CollectibleUi>();
            //_weaponUi = this.GetFirstChildOfType<WeaponHud>();

            if (_collectibleUi == null)
            {
                GD.PushError("Collectible UI node not found!");
            }

            // if (_weaponUi == null)
            // {
            //     GD.PushError("Weapon UI node not found!");
            // }
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

        private void UpdateAllUi(PlayerData playerData)
        {
            UpdateCollectibleUi(playerData.GetCollectibleCounts());
            UpdateWeaponUi();
        }
    }
}
