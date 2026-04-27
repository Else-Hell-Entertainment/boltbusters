// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    ///  Defines the price of an upgradable item.
    /// </summary>
    ///
    /// <seealso cref="IUpgradeable"/>
    [GlobalClass]
    public partial class PriceInfo : Resource
    {
        /// <summary>
        ///  The type of collectible that is required to purchase this item.
        /// </summary>
        ///
        /// <seealso cref="Collectible"/>
        [Export]
        public CollectibleType RequiredItem { get; set; } = CollectibleType.None;

        /// <summary>
        ///  The amount of the <see cref="RequiredItem"/> to be able to make
        ///  the purchase.
        /// </summary>
        ///
        /// <seealso cref="PlayerData.GetCollectibleCount"/>
        /// <seealso cref="PlayerData.GetCollectibleCounts"/>
        [Export]
        public int RequiredAmount { get; set; } = 0;
    }
}
