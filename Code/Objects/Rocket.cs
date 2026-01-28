using System;
using Godot;

public partial class Rocket : CharacterBody3D
{
    [Export]
    private float _speed = 20.0f;

    private bool _isActive = true;
    private MeshInstance3D _explosionMesh;

    public override void _Ready()
    {
        _explosionMesh = GetNode<MeshInstance3D>("ExplosionMesh");
        _explosionMesh.Visible = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isActive)
        {
            Vector3 direction = -GlobalTransform.Basis.Z;
            direction *= _speed;
            var collision = MoveAndCollide(direction * (float)delta);
            if (collision != null)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        _isActive = false;
        GD.Print("BOOM");
        _explosionMesh.Visible = true;
    }
}
