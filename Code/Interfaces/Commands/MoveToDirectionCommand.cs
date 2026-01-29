using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Command that moves an entity in a specified direction.
    /// Implements the Command pattern to encapsulate movement logic.
    /// </summary>
    public class MoveToDirectionCommand(Vector3 direction) : ICommand
    {
        /// <summary>
        /// Gets the direction vector in which the entity should move.
        /// </summary>
        public Vector3 Direction { get; } = direction;

        /// <summary>
        /// The EntityMover component that will execute the movement.
        /// </summary>
        private EntityMover _mover;

        /// <summary>
        /// Assigns an EntityMover target to this command. If no mover is assigned, the
        /// command does nothing.
        /// </summary>
        /// <param name="target">The target object, must be of type EntityMover.</param>
        /// <returns>True if the target is a valid EntityMover; otherwise, false.</returns>
        public bool AssignReceiver(object target)
        {
            if (target is not EntityMover mover)
                return false;
            _mover = mover;
            return true;
        }

        /// <summary>
        /// Executes the movement command. If mover is not assigned correctly, nothing happens.
        /// </summary>
        public void Execute()
        {
            _mover?.MoveToDirection(Direction);
        }
    }
}
