// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano <tuominen.mika-95@hotmail.com>

using Godot;

namespace EHE.BoltBusters
{
    public partial class Door : Node3D
    {
        [Export]
        private DoorShaderComponent _doorShaderComponent;

        public override void _Ready()
        {
            _doorShaderComponent.PlayDoorPulse();
        }
    }
}
