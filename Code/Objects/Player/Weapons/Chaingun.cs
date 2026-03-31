// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>

using System;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Controls a single chaingun. The player will have multiple of these under a chaingun controller class. Because
    /// of rapid fire rate and multiple chainguns, the controller manages the SFX and individual firing sequences of
    /// a group of chainguns.
    /// </summary>
    public partial class Chaingun : BaseWeapon
    {
        [Export]
        private Timer _cooldownTimer;

        [Export]
        private float _cooldown = 0.5f;

        [Export]
        private float _accuracy = 0.005f;

        [Export]
        private float _range = 9f;

        [Export]
        private MeshInstance3D _bulletTrail;

        private CylinderMesh _bulletMesh;

        /// <summary>
        /// Cooldown for a single chaingun can never be faster than one physics frame at 30 fps = 0.033 seconds.
        /// For a node with 8 guns (game default) working at 30 fps = minimum cooldown of 0.267 seconds is recommended.
        /// </summary>
        public float Cooldown
        {
            get => _cooldown;
            set => _cooldown = Mathf.Clamp(value, 0.034f, _cooldown);
        }

        private GpuParticles3D _hitParticles;
        private Node3D _muzzle;
        private DamageData _damageData;

        public override void _Ready()
        {
            // TODO: Placeholder implementation - refactor.
            _damageData = new DamageData(6, DamageType.Chaingun);

            _muzzle = GetNode<Node3D>("Muzzle");
            _hitParticles = GetNode<GpuParticles3D>("HitParticles");

            _cooldownTimer.WaitTime = _cooldown;
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
            _cooldownTimer.OneShot = true;
            SetTarget();

            _bulletMesh = (CylinderMesh)_bulletTrail.GetMesh();
            if (_bulletMesh == null)
            {
                GD.PrintErr("Couldn't find a bullet trail mesh!");
            }
        }

        private void SetTarget()
        {
            Vector3 targetPos = _muzzle.GlobalPosition;
            targetPos.Z -= _range;
            targetPos.Y = 0;
            _muzzle.LookAt(targetPos);
            // TODO: Is there a better solution?
            // This line makes sure the chaingun points to the right direction
            // on the XY plane. Without this, the newly added chainguns will point
            // to the player's default facing direction.
            _muzzle.Rotation = new Vector3(_muzzle.Rotation.X, 0, 0);
        }

        private void OnCooldownTimerTimeout()
        {
            CanAttack = true;
        }

        public override void Attack()
        {
            CanAttack = false;
            _cooldownTimer.Start();
            DoRayCast();
        }

        /// <summary>
        /// Passes a reference to the DamageData object to the receiving IDamageable.
        /// </summary>
        /// <param name="target"></param>
        private void ApplyDamage(IDamageable target)
        {
            target.TakeDamage(_damageData);
        }

        /// <summary>
        /// Draws a bullet trail effect (a cylinder mesh defined in editor) from the start to end position.
        /// </summary>
        /// <param name="start">Point in global space where the trail starts from.</param>
        /// <param name="end">Point in global space where the trail ends.</param>
        private void DrawBulletTrailEffect(Vector3 start, Vector3 end)
        {
            Vector3 direction = end - start;
            _bulletTrail.Show();
            _bulletTrail.GlobalPosition = start + direction * _bulletMesh.Height / 2;
            var transform = Transform3D.Identity;
            transform.Basis.Y = direction;
            _bulletTrail.Basis = transform.Basis;
            Tween bulletTween = CreateTween();
            bulletTween.TweenProperty(_bulletTrail, "position", end, 0.1f);
            bulletTween.TweenCallback(Callable.From(_bulletTrail.Hide));
        }

        /// <summary>
        /// Will raycast towards a preset target. Applies a small inaccuracy. If the raycast hits an IDamageable target,
        /// it will call ApplyDamage().
        /// Implementation still WIP.
        /// </summary>
        private void DoRayCast()
        {
            float vertDeviation = (float)GD.RandRange(-_accuracy, _accuracy);
            float horizontalDeviation = (float)GD.RandRange(-_accuracy, _accuracy);
            Vector3 deviation = new Vector3(horizontalDeviation, vertDeviation, 0);

            var spaceState = GetWorld3D().DirectSpaceState;
            Vector3 start = _muzzle.GlobalPosition;
            Vector3 direction = -_muzzle.GlobalBasis.Z;
            direction += deviation;
            Vector3 end = start + direction.Normalized() * 100f;

            var query = PhysicsRayQueryParameters3D.Create(start, end);
            query.CollideWithAreas = true;
            var result = spaceState.IntersectRay(query);
            if (result.ContainsKey("position"))
            {
                var collider = result["collider"];

                Node target = (Node)collider;
                Vector3 point = (Vector3)result["position"];
                Vector3 normal = (Vector3)result["normal"];
                PlayHitParticles(point, normal);
                DrawBulletTrailEffect(start, point);
                if (target is IDamageable damageable)
                {
                    ApplyDamage(damageable);
                }
            }
        }

        /// <summary>
        /// Emits hit particles from the hit point bounced from the surface normal.
        /// </summary>
        /// <param name="point">Point in global space where hit occurs.</param>
        /// <param name="normal">Normal of the surface.</param>
        private void PlayHitParticles(Vector3 point, Vector3 normal)
        {
            _hitParticles.GlobalPosition = point;
            ParticleProcessMaterial material = (ParticleProcessMaterial)_hitParticles.ProcessMaterial;
            Vector3 direction = (point - _muzzle.GlobalPosition).Normalized();
            Vector3 reflection = direction.Bounce(normal);

            // Convert reflection to local space of the particle emitter
            Vector3 localReflection = _hitParticles.GlobalTransform.Basis.Inverse() * reflection;
            material.Direction = localReflection;

            _hitParticles.Restart();
        }

        #region obsolete

        /// <summary>
        /// Draws a bullet trail. Placeholder implementation. Marked obsolete.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="direction"></param>
        /// <param name="end"></param>
        [Obsolete]
        private void DrawBulletTrail(Vector3 start, Vector3 direction, Vector3 end)
        {
            var lineMesh = new MeshInstance3D();
            var cylinderMesh = new CylinderMesh();
            var lineMaterial = new OrmMaterial3D();
            var lineTransform = Transform3D.Identity;
            var lineLength = 200;

            lineMesh.Mesh = cylinderMesh;
            lineMesh.Position = start + direction * lineLength / 2;
            lineMesh.MaterialOverride = lineMaterial;
            lineMesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

            cylinderMesh.Rings = 0;
            cylinderMesh.RadialSegments = 6;
            cylinderMesh.Height = lineLength;
            cylinderMesh.TopRadius = 0.004f;
            cylinderMesh.BottomRadius = 0.004f;

            lineMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            lineMaterial.AlbedoColor = Colors.Gold;

            lineTransform.Basis.Y = direction;
            lineTransform.Basis.X = Vector3.Up.Cross(direction).Normalized();
            lineTransform.Basis.Z = lineTransform.Basis.X.Cross(direction).Normalized();
            lineMesh.Basis = lineTransform.Basis;

            Tween lineTween = CreateTween();
            Vector3 lineEndPos = end - direction * lineLength / 2;
            lineTween.TweenProperty(lineMesh, "position", lineEndPos, 10.0f);
            GetTree().GetRoot().AddChild(lineMesh);
        }

        #endregion
    }
}
