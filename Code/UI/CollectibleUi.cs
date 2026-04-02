// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System.Collections.Generic;
using EHE.Common.Godot.Extensions;
using Godot;

namespace EHE.BoltBusters.Ui
{
    /// <summary>
    ///  A container for collectible counters in the HUD.
    /// </summary>
    ///
    /// <remarks>
    ///  This node is dependent on the <see cref="GameManager.CurrentPlayerData"/>
    ///  in <see cref="GameManager"/>. The CollectibleUi node subscribes to the
    ///  <see cref="PlayerData.CollectibleCountChanged"/> signal when it enters
    ///  the scene tree. This signal is used to instruct the
    ///  <see cref="CollectibleCounter"/> nodes under the CollectibleUi node to
    ///  update their display values. If the signal cannot be connected, an
    ///  error is logged.
    /// </remarks>
    ///
    /// <seealso cref="ICollectible"/>
    /// <seealso cref="Collectible"/>
    /// <seealso cref="CollectibleCounter"/>
    /// <seealso cref="PlayerData"/>
    public partial class CollectibleUi : BoxContainer
    {
        // Internal mappings for collectible types and their counters.
        private Godot.Collections.Dictionary<CollectibleType, CollectibleCounter> _collectibleCounters = new();

        /// <summary>
        ///  Connects the required signals. Logs an error if this fails.
        /// </summary>
        public override void _EnterTree()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentPlayerData == null)
            {
                GD.PushError(
                    $"Cannot connect signals. "
                        + $"{nameof(GameManager.Instance)} or {nameof(GameManager.Instance.CurrentPlayerData)} is null!"
                );
                return;
            }

            GameManager.Instance.CurrentPlayerData.CollectibleCountChanged += SetCollectibleCount;
        }

        /// <summary>
        ///  Disconnects the required signals.
        /// </summary>
        public override void _ExitTree()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentPlayerData != null)
            {
                GameManager.Instance.CurrentPlayerData.CollectibleCountChanged -= SetCollectibleCount;
            }
        }

        public override void _Ready()
        {
            CacheCounters();
        }

        /// <summary>
        ///  Finds all counter nodes and creates mappings for them using their
        ///  <see cref="CollectibleCounter.CollectibleType"/> as keys. If two or
        ///  more counters with the same collectible type are found, the one
        ///  that was found first is added to the mapping and a warning is
        ///  logged for the rest of them.
        /// </summary>
        private void CacheCounters()
        {
            foreach (var counter in this.GetChildrenOfType<CollectibleCounter>(recurse: true))
            {
                if (!_collectibleCounters.TryAdd(counter.CollectibleType, counter))
                {
                    GD.PushWarning($"Cannot cache counter for type '{counter.CollectibleType}'. Already added?");
                }
            }

            if (_collectibleCounters.Count == 0)
            {
                GD.PushWarning("No counters found in children. Is this intended?");
            }
        }

        /// <summary>
        ///  Sets the counter value for the given counter.
        /// </summary>
        ///
        /// <param name="type">
        ///  The type of the collectible whose counter should be set.
        /// </param>
        /// <param name="value">
        ///  The new value shown in the counter.
        /// </param>
        private void SetCollectibleCount(int type, int value)
        {
            var collectibleType = (CollectibleType)type;

            if (!_collectibleCounters.TryGetValue(collectibleType, out var counter))
            {
                GD.PushError($"Counter for '{collectibleType}' not found in {nameof(_collectibleCounters)}");
                return;
            }

            counter.SetCounterValue(value);
        }
    }
}
