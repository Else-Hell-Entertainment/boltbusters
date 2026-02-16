using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Command that rotates an entity to face a specified point in 3D space.
    /// Implements the Command pattern to encapsulate rotation logic.
    /// </summary>
    public class RotateToDirectionCommand(Vector3 point) : ICommand
    {
        /// <summary>
        /// Gets the target point in 3D space that the entity should rotate towards.
        /// </summary>
        public Vector3 Point { get; } = point;

        /// <summary>
        /// The EntityMover component that will execute the rotation.
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
        /// Executes the rotation command. If mover is not assigned correctly, nothing happens.
        /// </summary>
        public void Execute()
        {
            _mover?.RotateToDirection(Point);
        }
    }
}
