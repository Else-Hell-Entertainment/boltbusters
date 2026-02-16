using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Concrete implementation of EntityMover for CharacterBody3D nodes.
    /// Handles physics-based movement using Godot's built-in CharacterBody3D collision system.
    /// </summary>
    public class CB3DMover(CharacterBody3D body) : EntityMover
    {
        /// <summary>
        /// The CharacterBody3D that this mover controls.
        /// </summary>
        private CharacterBody3D _body = body;

        /// <summary>
        /// The speed at which the entity moves, measured in units per second.
        /// </summary>
        [Export]
        public float MovementSpeed = 10f;

        /// <summary>
        /// The speed at which the entity rotates, measured in radians per second.
        /// Currently unused but available for future rotation implementation.
        /// </summary>
        [Export]
        public float RotationSpeed = 8.0f;

        [Export]
        public float Acceleration = 40f;

        /// <summary>
        /// Moves the entity in the specified direction using physics-based movement.
        /// Applies velocity on the XZ plane and uses MoveAndSlide for collision handling.
        /// </summary>
        /// <param name="direction">The direction vector to move towards (Y component is ignored).</param>
        public override void MoveToDirection(Vector3 direction)
        {
            if (direction == Vector3.Zero)
            {
                _body.Velocity = Vector3.Zero;
                _body.MoveAndSlide();
                return;
            }
            // Create normalized direction vector on XZ plane
            direction.Y = 0;
            direction = direction.Normalized();
            Vector3 desiredVelocity = direction * MovementSpeed;
            float dt = (float)_body.GetPhysicsProcessDeltaTime();
            _body.Velocity = _body.Velocity.MoveToward(desiredVelocity, Acceleration * dt);
            _body.MoveAndSlide();
        }
    }
}
