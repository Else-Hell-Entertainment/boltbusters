using System;
using EHE.BoltBusters;
using Godot;
using Godot.Collections;

public partial class Rocket : CharacterBody3D
{
    [Export]
    private float _speed = 20.0f;

    private bool _isActive = true;
    private MeshInstance3D _explosionMeshInstance;
    private SphereMesh _explosionMesh;
    private ShapeCast3D _explosionCast;
    private MeshInstance3D _rocketBodyMeshInstance;
    private DamageData _damageData;

    public override void _Ready()
    {
        _explosionMeshInstance = GetNode<MeshInstance3D>("ExplosionMesh");
        _explosionMesh = (SphereMesh)_explosionMeshInstance.Mesh;
        _explosionMesh.Radius = 0;
        _explosionMesh.Height = 0;
        _explosionMeshInstance.Visible = false;
        _explosionCast = GetNode<ShapeCast3D>("ExplosionCast");
        _rocketBodyMeshInstance = GetNode<MeshInstance3D>("RocketBodyMesh");
        _damageData = new DamageData(50, DamageType.Missile);
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
        CheckExplosionDamage();
        _isActive = false;
        GD.Print("BOOM");
        HideRocketBody();
        _explosionMeshInstance.Visible = true;
        Tween _explosionTween = CreateTween();
        _explosionTween.TweenProperty(_explosionMesh, "radius", 2.8, 0.05);
        _explosionTween.Parallel().TweenProperty(_explosionMesh, "height", 5.6, 0.05);
        _explosionTween.TweenCallback(Callable.From(Reset));
    }

    private void CheckExplosionDamage()
    {
        _explosionCast.ForceShapecastUpdate();
        var collisions = _explosionCast.CollisionResult;
        foreach (Dictionary collision in collisions)
        {
            if (collision.ContainsKey("collider"))
            {
                var collider = collision["collider"];
                Node target = (Node)collider;
                if (target is IDamageable damageable)
                {
                    damageable.TakeDamage(_damageData);
                    GD.Print("Rocket did damage.");
                }
            }
        }
    }

    private void HideRocketBody()
    {
        _rocketBodyMeshInstance.Visible = false;
    }

    private void Reset()
    {
        _explosionMesh.Radius = 0;
        _explosionMesh.Height = 0;
        _explosionMeshInstance.Visible = false;
        _rocketBodyMeshInstance.Visible = true;
    }
}
