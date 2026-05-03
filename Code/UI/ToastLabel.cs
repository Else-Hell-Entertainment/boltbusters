// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using Godot;

namespace EHE.BoltBusters.Ui
{
    [GlobalClass]
    public partial class ToastLabel : Label
    {
        [Export]
        private double _displayTime = 5.0;

        private readonly Timer _displayTimer = new();

        public override void _EnterTree()
        {
            _displayTimer.Timeout += Hide;
        }

        public override void _ExitTree()
        {
            _displayTimer.Timeout -= Hide;
        }

        public override void _Ready()
        {
            _displayTimer.SetName("DisplayTimer");
            _displayTimer.WaitTime = _displayTime;
            AddChild(_displayTimer);
            Hide();
        }

        public void Toast()
        {
            Show();
            _displayTimer.Start();
        }
    }
}
