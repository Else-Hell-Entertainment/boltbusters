// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>

using Godot;
using Godot.Collections;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Prototype rocket. WIP.
    /// </summary>
    public partial class Rocket : CharacterBody3D
    {
        [Export]
        private float _speed = 20.0f;

        private bool _isActive = true;
        private SphereMesh _explosionMesh;
        private ShapeCast3D _explosionCast;
        private MeshInstance3D _rocketBodyMeshInstance;
        private DamageData _damageData;

        [Export]
        private AnimationPlayer _VFXanimationPlayer;

        public override void _Ready()
        {
            _explosionCast = GetNode<ShapeCast3D>("ExplosionCast");
            _rocketBodyMeshInstance = GetNode<MeshInstance3D>("RocketBodyMesh");
            _damageData = new DamageData(50, DamageType.Missile);
            _VFXanimationPlayer = GetNode<AnimationPlayer>("VFX/Explosion/AnimationPlayer");
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
            CallDeferred(MethodName.CheckExplosionDamage);
            _VFXanimationPlayer.Play("Explode");
            _isActive = false;
            HideRocketBody();
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
            _rocketBodyMeshInstance.Visible = true;
        }
    }
}
