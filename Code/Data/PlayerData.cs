// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System;
using Godot;
using Godot.Collections;

namespace EHE.BoltBusters
{
    [GlobalClass]
    public partial class PlayerData : Resource
    {
        #region Signals

        /// <summary>
        ///  Emitted when the <see cref="Health"/> property changes.
        /// </summary>
        ///
        /// <param name="newHealth">
        ///  The new value of <see cref="Health"/>.
        /// </param>
        [Signal]
        public delegate void HealthChangedEventHandler(int newHealth);

        #endregion Signals


        #region Fields (private/protected)

        private int _health;

        #endregion Private Fields


        #region Exported Fields & Properties (private/protected/public)

        /// <summary>
        ///  The current health of the player.
        /// </summary>
        ///
        /// <remarks>
        ///  The <paramref name="value"/> is clamped between 0 and
        ///  <see cref="int.MaxValue"/>. <b>Note</b>: The maximum value will
        ///  be lowered when the UI is implemented.
        /// </remarks>
        [Export(PropertyHint.Range, "0,2147483647,1")]
        public int Health
        {
            get => _health;
            set
            {
                // TODO: Decide max value when designing UI.
                _health = Mathf.Clamp(value, min: 0, max: int.MaxValue);
                EmitSignal(SignalName.HealthChanged, _health);
            }
        }

        #endregion Exported Fields & Properties (private/protected/public)
    }
}
