using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Controls a single chaingun. The player will have multiple of these under a chaingun controller class. Allows
    /// for the chaingun to be used as independent weapon or as a part of controlled group (implementation still WIP).
    /// Implementation of this feature is still WIP.
    /// </summary>
    public partial class Chaingun : BaseWeapon
    {
        [Export]
        private Timer _cooldownTimer;

        [Export]
        private float _cooldown = 0.5f;

        [Export]
        private float _accuracy = 0.05f;

        [Export]
        private float _range = 7f;

        [Export]
        private AudioStreamPlayer3D _audioStreamPlayer;

        /// <summary>
        /// Cooldown for a single chaingun can never be faster than one physics frame at 30 fps = 0.033 seconds.
        /// For a node with 8 guns (game default) working at 30 fps = minimum cooldown of 0.267 seconds is recommended.
        /// </summary>
        public float Cooldown
        {
            get => _cooldown;
            set => _cooldown = Mathf.Clamp(value, 0.034f, _cooldown);
        }

        private bool _canFire = true;
        private GpuParticles3D _hitParticles;
        private Node3D _muzzle;
        private MeshInstance3D _reticle;
        private DamageData _damageData;

        public override void _Ready()
        {
            // TODO: Placeholder implementation - refactor.
            _damageData = new DamageData(100, DamageType.Chaingun);

            _muzzle = GetNode<Node3D>("Muzzle");
            _hitParticles = GetNode<GpuParticles3D>("HitParticles");
            _reticle = GetNode<MeshInstance3D>("Reticle");

            _cooldownTimer.WaitTime = _cooldown;
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
            _cooldownTimer.OneShot = true;
            SetTarget();
        }

        private void SetTarget()
        {
            Vector3 targetPos = _muzzle.GlobalPosition;
            targetPos.Z = _muzzle.Position.Z - _range;
            targetPos.Y = 0.2f;
            _muzzle.LookAt(targetPos);
            _reticle.GlobalPosition = targetPos;
        }

        private void OnCooldownTimerTimeout()
        {
            _canFire = true;
        }

        public override bool CanAttack()
        {
            return _canFire;
        }

        public override void Attack()
        {
            _canFire = false;
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
            GD.Print("Chaingun did damage!");
        }

        /// <summary>
        /// Draws a bullet trail. Placeholder implementation.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="direction"></param>
        /// <param name="end"></param>
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
            Vector3 end = start + direction.Normalized() * 1000f;

            var query = PhysicsRayQueryParameters3D.Create(start, end);
            query.CollideWithAreas = true;
            var result = spaceState.IntersectRay(query);
            DrawBulletTrail(start, direction, end);
            if (result.ContainsKey("position"))
            {
                var collider = result["collider"];

                Node target = (Node)collider;
                Vector3 point = (Vector3)result["position"];
                _hitParticles.GlobalPosition = point;
                _hitParticles.Emitting = true;
                if (target is IDamageable damageable)
                {
                    ApplyDamage(damageable);
                }
            }
        }
    }
}
