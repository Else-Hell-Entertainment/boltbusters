// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using EHE.Common.Godot.Extensions;
using Godot;

namespace EHE.BoltBusters.Ui
{
    public abstract partial class Menu : Control
    {
        private Control _focusStart;

        public Menu()
        {
            // Ensures that no menu can become locked when the game is paused.
            ProcessMode = ProcessModeEnum.Always;
        }

        /// <summary>
        ///  Grabs the focus and enables keyboard and controller input.
        /// </summary>
        public override void _EnterTree()
        {
            FocusStart();
        }

        public void FocusStart()
        {
            _focusStart ??= this.GetFirstChildOfType<Range>(recurse: true);
            _focusStart ??= this.GetFirstChildOfType<Button>(recurse: true);
            _focusStart.CallDeferred(Control.MethodName.GrabFocus);
        }
    }
}
