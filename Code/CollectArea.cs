// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano tuominen.mika-95@hotmail.com

using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Detects collectible objects that enter the player's collection radius
    /// and triggers their collection logic.
    /// </summary>
    /// <remarks>
    /// This component should be attached as a child of the <see cref="Player"/> node.
    /// It listens for <see cref="Area3D"/> bodies entering its space and attempts
    /// to collect any objects that implement <see cref="ICollectible"/>.
    /// </remarks>
    public partial class CollectArea : Area3D
    {
        private Area3D _collectArea = null;
        private Player _player = null;

        public override void _Ready()
        {
            _player = GetParent<Player>();
            _collectArea = this;

            _collectArea.AreaEntered += OnAreaEntered;
        }

        /// <summary>
        /// Called when another <see cref="Area3D"/> enters the collection area.
        /// Forwards the event to <see cref="TryCollect"/>.
        /// </summary>
        /// <param name="area">The area that entered the collection zone.</param>
        private void OnAreaEntered(Area3D area)
        {
            TryCollect(area);
        }

        /// <summary>
        /// Checks whether the specified area implements <see cref="ICollectible"/>.
        /// If so, triggers its <see cref="ICollectible.OnCollect"/> method
        /// using the player as the collector.
        /// </summary>
        /// <param name="area">The area to check for collectible behavior.</param>
        private void TryCollect(Area3D area)
        {
            if (area is ICollectible collectible)
            {
                collectible.OnCollect(_player);
            }
        }
    }
}
