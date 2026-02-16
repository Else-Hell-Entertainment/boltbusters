using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Concrete implementation of EntityMover that operates on a Node3D.
    /// Handles movement and rotation for 3D node-based entities in the game world.
    /// </summary>
    public class Node3DMover(Node3D node) : EntityMover
    {
        /// <summary>
        /// The Node3D body that this mover controls.
        /// </summary>
        [Export]
        private Node3D _body = node;

        /// <summary>
        /// The speed at which the entity moves, measured in units per second.
        /// </summary>
        [Export]
        private float _movementSpeed = 10f;

        /// <summary>
        /// The speed at which the entity rotates, measured in radians per second.
        /// </summary>
        [Export]
        private float _rotationSpeed = 8.0f;

        /// <summary>
        /// Moves the entity in the specified direction.
        /// Currently not implemented.
        /// </summary>
        /// <param name="direction">The normalized direction vector to move towards.</param>
        public override void MoveToDirection(Vector3 direction) { }

        /// <summary>
        /// Moves the entity to a specific position in 3D space.
        /// Currently not implemented.
        /// </summary>
        /// <param name="point">The target position to move to.</param>
        public override void MoveToPosition(Vector3 point) { }

        /// <summary>
        /// Rotates the entity to face a specific point in global 3D space.
        /// Uses smooth rotation with a maximum rotation speed to prevent instant snapping.
        /// </summary>
        /// <param name="point">The point in global 3D space to rotate towards.</param>
        public override void RotateTowards(Vector3 point)
        {
            // Get delta time for frame-rate independent rotation
            float dt = (float)_body.GetPhysicsProcessDeltaTime();

            // Calculate direction to target on XZ plane (ignore Y component)
            Vector3 toTarget = point - _body.GlobalPosition;
            toTarget.Y = 0.0f;

            // Get the entity's current forward direction
            Vector3 forward = -_body.GlobalTransform.Basis.Z;

            // Calculate signed angle between forward and target direction
            float angleTo = forward.SignedAngleTo(toTarget, Vector3.Up);

            // Clamp rotation to maximum rotation speed
            float maxRotationDelta = _rotationSpeed * dt;
            float rotationDelta = Mathf.Clamp(angleTo, -maxRotationDelta, maxRotationDelta);

            // Apply rotation around the up axis
            _body.Rotate(Vector3.Up, rotationDelta);
        }

        public override void RotateToDirection(Vector3 direction)
        {
            Vector3 point = _body.GlobalPosition + direction;
            RotateTowards(point);
        }
    }
}
