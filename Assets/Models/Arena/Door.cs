using System;
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
