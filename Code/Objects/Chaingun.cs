using Godot;

namespace EHE.BoltBusters
{
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

        private bool _canFire = true;

        private GpuParticles3D _hitParticles;
        private Node3D _muzzle;
        private MeshInstance3D _reticle;

        // TODO: HACK: HORRIBLE: TESTING ONLY: TEMPORARY!!!!
        [Export]
        private MeshInstance3D _effect;

        [Export]
        private Timer _effectTimer;

        [Export]
        private float _effectTime = 0.05f;

        public override void _Ready()
        {
            _muzzle = GetNode<Node3D>("Muzzle");
            _hitParticles = GetNode<GpuParticles3D>("HitParticles");
            _reticle = GetNode<MeshInstance3D>("Reticle");
            //Vector3 retPos = _muzzle.Position + new Vector3(0, 0, -_range);
            //_reticle.Position = retPos;

            _cooldownTimer.WaitTime = _cooldown;
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
            _cooldownTimer.OneShot = true;

            _effectTimer.WaitTime = _effectTime;
            _effectTimer.Timeout += ResetEffect;
            _effect.Visible = false;

            SetTarget();
        }

        private void SetTarget()
        {
            Vector3 targetPos = _muzzle.GlobalPosition;
            targetPos.Z = _muzzle.Position.Z - _range;
            targetPos.Y = 0.2f;
            //Vector3 dir = _muzzle.ToGlobal(targetPos);
            _muzzle.LookAt(targetPos);
            _reticle.GlobalPosition = targetPos;
        }

        private void ResetEffect()
        {
            _effect.Visible = false;
        }

        private void OnCooldownTimerTimeout()
        {
            GD.Print("Chaingun ready to fire.");
            _canFire = true;
        }

        public override bool CanAttack()
        {
            return _canFire;
        }

        public override void Attack()
        {
            GD.Print("ChainGun goes PewPewPew");
            GD.Print("More pew.");
            _canFire = false;
            _cooldownTimer.Start();

            _effectTimer.Start();
            DoRayCast();
            //_effect.Visible = true;
        }

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
            GD.Print(result);
            DrawBulletTrail(start, direction, end);
            if (result.ContainsKey("position"))
            {
                GD.Print("Hit: " + result["position"]);
                var collider = result["collider"];

                Node targ = (Node)collider;
                Vector3 point = (Vector3)result["position"];
                _hitParticles.GlobalPosition = point;
                _hitParticles.Emitting = true;
                /*
                if (targ.IsInGroup("Enemy"))
                {
                    Vector3 point = (Vector3)result["position"];
                    _hitParticles.GlobalPosition = point;
                    _hitParticles.Emitting = true;
                }
                */
            }
        }

        private void Shoot() { }
    }
}
