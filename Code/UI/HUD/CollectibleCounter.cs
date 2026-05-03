// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.Common.Godot.Extensions;
using Godot;

namespace EHE.BoltBusters.Ui
{
    /// <summary>
    ///  A counter that tracks one specific collectible defined by its
    ///  <see cref="CollectibleType"/>.
    /// </summary>
    ///
    /// <remarks>
    ///  This node is intended to be placed under the
    ///  <see cref="CollectibleUi"/> node in the scene tree.
    /// </remarks>
    ///
    /// <seealso cref="Collectible"/>
    /// <seealso cref="ICollectible"/>
    public partial class CollectibleCounter : BoxContainer
    {
        private TextureRect _iconRect;
        private Label _valueLabel;

        /// <summary>
        ///  The type of collectible this counter keeps track of.
        ///  The default value is <see cref="CollectibleType.None"/>.
        /// </summary>
        [Export]
        public CollectibleType CollectibleType { get; private set; } = CollectibleType.None;

        public override void _Ready()
        {
            _iconRect = this.GetFirstChildOfType<TextureRect>();
            _valueLabel = this.GetFirstChildOfType<Label>();

            if (_iconRect == null)
            {
                GD.PushError($"There is no valid node for {nameof(_iconRect)}!");
            }

            if (_valueLabel == null)
            {
                GD.PushError($"There is no valid node for {nameof(_valueLabel)}!");
            }
        }

        /// <summary>
        ///  Sets the display value for the counter.
        /// </summary>
        ///
        /// <param name="value">The new value to display.</param>
        public void SetCounterValue(int value)
        {
            _valueLabel.Text = $"{value}";
        }
    }
}
