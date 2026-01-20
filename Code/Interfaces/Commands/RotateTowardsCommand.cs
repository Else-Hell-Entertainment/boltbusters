using Godot;

namespace EHE.BoltBusters
{
    public class RotateTowardsCommand(Vector3 point) : ICommand
    {
        public CB3DMover Mover = null;
        private Vector3 _point = point;

        public void Execute() { }

        public void Execute(CB3DMover mover)
        {
            mover?.RotateTowards(_point);
        }
    }
}
