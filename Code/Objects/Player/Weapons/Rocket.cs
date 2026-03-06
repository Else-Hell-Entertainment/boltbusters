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
    public partial class Rocket : Projectile
    {
        public bool IsAvailable { get; private set; } = true;

        private const int COLLISION_MASK_LAYER = 1;

        [Export]
        private float _speed = 50.0f;
        [Export]
        private AudioStreamPlayer3D _launchSound;
        [Export]
        private AudioStreamPlayer3D _exlosionSound;

        private CharacterBody3D _rocketBody;

        private bool _isActive = true;
        private SphereMesh _explosionMesh;
        private ShapeCast3D _explosionCast;
        private MeshInstance3D _rocketBodyMeshInstance;
        private DamageData _damageData;

        [Export]
        private AnimationPlayer _vfxAnimationPlayer;

        public override void _Ready()
        {
            InitializeNodes();
            VerifyInit();
            _vfxAnimationPlayer.AnimationFinished += OnExplosionVfxFinished;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isActive)
            {
                Move(delta);
            }
        }

        private void Move(double delta)
        {
            Vector3 direction = -_rocketBody.GlobalTransform.Basis.Z;
            direction *= _speed;
            var collision = _rocketBody.MoveAndCollide(direction * (float)delta);
            if (collision != null)
            {
                Explode();
            }
        }

        private void InitializeNodes()
        {
            _rocketBody = GetNodeOrNull<CharacterBody3D>("RocketBody");
            _explosionCast = _rocketBody.GetNodeOrNull<ShapeCast3D>("ExplosionCast");
            _rocketBodyMeshInstance = _rocketBody.GetNodeOrNull<MeshInstance3D>("RocketBodyMesh");
            _damageData = new DamageData(50, DamageType.Missile);
            _vfxAnimationPlayer = _rocketBody.GetNodeOrNull<AnimationPlayer>("VFX/Explosion/AnimationPlayer");
        }

        private void VerifyInit()
        {
            if (_rocketBody == null)
            {
                GD.PrintErr("Rocket: No rocket body found.");
            }

            if (_rocketBodyMeshInstance == null)
            {
                GD.PrintErr("Rocket: No rocket body mesh instance found.");
            }
            if (_damageData == null)
            {
                GD.PrintErr("Rocket: No damage data found.");
            }

            if (_vfxAnimationPlayer == null)
            {
                GD.PrintErr("Rocket: No animation player found.");
            }
        }

        public void LaunchRocket(Node3D launchPoint, Vector3 direction)
        {
            _isActive = true;
            IsAvailable = false;
            _rocketBody.SetCollisionMaskValue(COLLISION_MASK_LAYER, true);
            _rocketBodyMeshInstance.Visible = true;
            Vector3 globalDir = launchPoint.GlobalBasis * direction;
            globalDir = globalDir.Normalized();
            _rocketBody.GlobalPosition = launchPoint.GlobalPosition;
            Transform3D t = _rocketBody.GlobalTransform;
            t.Basis = Basis.LookingAt(globalDir, Vector3.Up);
            _rocketBody.GlobalTransform = t;
            _launchSound.Play();
        }

        private void Explode()
        {
            CallDeferred(MethodName.ResolveExplosionDamage);
            _vfxAnimationPlayer.Play("Explode");
            _exlosionSound.Play();
            DeactivateRocketBody();
        }

        private void ResolveExplosionDamage()
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
                    }
                }
            }
        }

        /// <summary>
        /// Deactivates the body of the rocket. Used when the rocket hits a target, but still needs to play the vfx.
        /// </summary>
        private void DeactivateRocketBody()
        {
            _isActive = false;
            _rocketBodyMeshInstance.Visible = false;
            _rocketBody.SetCollisionMaskValue(COLLISION_MASK_LAYER, false);
        }

        private void OnExplosionVfxFinished(StringName animation)
        {
            IsAvailable = true;
        }
    }
}
