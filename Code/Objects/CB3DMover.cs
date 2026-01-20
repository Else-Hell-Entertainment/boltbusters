using Godot;

namespace EHE.BoltBusters
{
    public class CB3DMover(CharacterBody3D characterBody3D)
    {
        private CharacterBody3D _characterBody3D = characterBody3D;

        [Export]
        public float MovementSpeed = 10f;

        [Export]
        public float RotationSpeed = 3.0f;

        public void MoveToDirection(Vector3 direction)
        {
            Vector3 dirVector = Vector3.Zero;
            dirVector.X = direction.X;
            dirVector.Z = direction.Z;
            dirVector = dirVector.Normalized();
            _characterBody3D.Velocity = dirVector * MovementSpeed;
            _characterBody3D.MoveAndSlide();
        }
    }
}
