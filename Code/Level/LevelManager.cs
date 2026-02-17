using EHE.BoltBusters.States;
using Godot;

namespace EHE.BoltBusters
{
    public partial class LevelManager : Node3D
    {
        public override void _Input(InputEvent inputEvent)
        {
            // TODO: Move the key name to a config file.
            if (inputEvent.IsActionPressed("Pause"))
            {
                GameManager.Instance.StateMachine.TransitionTo(StateType.MenuPause);
            }
        }
    }
}
