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
    /// <seealso cref="ICollectible"/>
    /// <seealso cref="Collectible"/>
    /// <seealso cref="CollectibleCounter"/>
    public partial class CollectibleUi : BoxContainer
    {
        // Internal mappings for collectible types and their counters.
        private Godot.Collections.Dictionary<CollectibleType, CollectibleCounter> _collectibleCounters = new();

        public override void _Ready()
        {
            CacheCounters();
        }

        /// <summary>
        ///  Sets the counter value for the given counter.
        /// </summary>
        ///
        /// <param name="collectibleType">
        ///  The type of the collectible whose counter should be set.
        /// </param>
        /// <param name="value">
        ///  The new value shown in the counter.
        /// </param>
        public void SetCollectibleCount(CollectibleType collectibleType, int value)
        {
            if (!_collectibleCounters.TryGetValue(collectibleType, out var counter))
            {
                GD.PushError($"Counter for '{collectibleType}' not found in {nameof(_collectibleCounters)}");
                return;
            }

            counter.SetCounterValue(value);
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
    }
}
