using Godot;

namespace EHE.BoltBusters
{
    public class MoveToDirectionCommand(Vector3 direction) : ICommand
    {
        public CB3DMover Mover = null;

        private Vector3 _direction = direction;

        public void Execute()
        {
            if (Mover != null)
            {
                Mover.MoveToDirection(_direction);
            }
            else
            {
                GD.PrintErr("No mover component found in MoveToDirectionCommand");
            }
        }

        public void Execute(CB3DMover mover)
        {
            mover?.MoveToDirection(_direction);
        }
    }
}
