// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters
{
    public class MoveToPositionCommand(Vector3 point) : ICommand
    {
        public Vector3 TargetPoint { get; } = point;

        private EntityMover _mover;

        public bool AssignReceiver(object target)
        {
            if (target is not EntityMover mover)
            {
                return false;
            }
            _mover = mover;
            return true;
        }

        public void Execute()
        {
            _mover?.MoveToPosition(TargetPoint);
        }
    }
}
