using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Abstract base class for entity movement and rotation logic.
    /// Provides virtual methods that can be overridden to implement specific movement behaviors.
    /// </summary>
    public abstract class EntityMover
    {
        /// <summary>
        /// Moves the entity in the specified direction.
        /// Override this method to implement directional movement logic.
        /// </summary>
        /// <param name="direction">The normalized direction vector to move towards.</param>
        public virtual void MoveToDirection(Vector3 direction) { }

        /// <summary>
        /// Moves the entity to a specific position in 3D space.
        /// Override this method to implement position-based movement logic.
        /// </summary>
        /// <param name="position">The target position to move to.</param>
        public virtual void MoveToPosition(Vector3 position) { }

        /// <summary>
        /// Rotates the entity to face a specific point in global 3D space.
        /// Override this method to implement rotation logic.
        /// </summary>
        /// <param name="point">The point in global 3D space to rotate towards.</param>
        public virtual void RotateTowards(Vector3 point) { }
    }
}
