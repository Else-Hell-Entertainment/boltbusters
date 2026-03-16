// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

namespace EHE.BoltBusters
{
    /// <summary>
    /// Command that stops movement on an <see cref="EntityMover"/> receiver.
    /// </summary>
    /// <remarks>
    /// Call <see cref="AssignReceiver(object)"/> before <see cref="Execute()"/> so the command
    /// has a valid target to operate on.
    /// </remarks>
    public class StopMovementCommand : ICommand
    {
        /// <summary>
        /// Receiver that performs the actual movement stop operation.
        /// </summary>
        private EntityMover _mover;

        /// <summary>
        /// Executes the command by stopping the assigned receiver's movement.
        /// </summary>
        public void Execute()
        {
            _mover.StopMovement();
        }

        /// <summary>
        /// Assigns the command receiver.
        /// </summary>
        /// <param name="target">Object expected to be an <see cref="EntityMover"/>.</param>
        /// <returns>
        /// <see langword="true"/> when <paramref name="target"/> is a valid receiver and assignment succeeds;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool AssignReceiver(object target)
        {
            if (target is not EntityMover mover)
            {
                return false;
            }
            _mover = mover;
            return true;
        }
    }
}
