using Godot;
using Godot.Collections;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Prototype Railgun. WIP.
    /// </summary>
    public partial class Railgun : BaseWeapon
    {
        public bool IsActive { get; set; }

        [Export]
        private Timer _cooldownTimer;

        [Export]
        private float _cooldown = 5f;

        private MeshInstance3D _laserSightInstance;
        private CylinderMesh _laserSightMesh;

        private MeshInstance3D _bulletEffect;
        private MeshInstance3D _bulletTravel;
        private Node3D _muzzle;
        private DamageData _damageData;
        private GpuParticles3D _hitParticles;

        private Dictionary _lastRaycastResult = new Dictionary();

        [Signal]
        public delegate void RailgunReloadReadyEventHandler(Railgun railgun);

        public override void _Ready()
        {
            _cooldownTimer = GetNode<Timer>("CooldownTimer");
            _cooldownTimer.WaitTime = _cooldown;
            _cooldownTimer.OneShot = true;
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
            _muzzle = GetNode<Node3D>("Muzzle");
            _damageData = new DamageData(200, DamageType.Sniper);
            _hitParticles = GetNode<GpuParticles3D>("HitParticles");
            _bulletEffect = GetNode<MeshInstance3D>("BulletEffect");
            _bulletTravel = GetNode<MeshInstance3D>("BulletTravel");
            _bulletEffect.Visible = false;
            _bulletTravel.Visible = false;

            _laserSightInstance = GetNode<MeshInstance3D>("LaserSight");
            _laserSightMesh = (CylinderMesh)_laserSightInstance.Mesh;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (IsActive)
            {
                _lastRaycastResult = RayCastForward();
                UpdateLaserSight();
            }
        }

        private void UpdateLaserSight()
        {
            if (_lastRaycastResult.ContainsKey("position"))
            {
                Vector3 point = (Vector3)_lastRaycastResult["position"];
                Vector3 direction = point - _muzzle.GlobalPosition;
                float distance = direction.Length();
                _laserSightMesh.Height = distance;
                Vector3 midpoint = _muzzle.GlobalPosition + direction * 0.5f;
                _laserSightInstance.GlobalPosition = midpoint;
                _laserSightInstance.Show();
            }
        }

        private void OnCooldownTimerTimeout()
        {
            CanAttack = true;
            EmitSignal(SignalName.RailgunReloadReady, this);
        }

        public override void Attack()
        {
            if (!CanAttack)
            {
                return;
            }
            CanAttack = false;
            _cooldownTimer.Start();
            _laserSightInstance.Hide();
            ResolveAttack();
            //DoRayCast();
            DrawBulletEffect();
        }

        private void ResolveAttack()
        {
            Vector3 origin = _muzzle.GlobalPosition;
            Vector3 direction = -_muzzle.GlobalBasis.Z;
            int maxPierceCount = 5;

            for (int i = 0; i < maxPierceCount; i++)
            {
                var result = Raycast(origin, direction);
                if (result.ContainsKey("position"))
                {
                    var collider = result["collider"];
                    Node target = (Node)collider;
                    Vector3 hit = (Vector3)result["position"];
                    _hitParticles.GlobalPosition = hit;
                    _hitParticles.Emitting = true;
                    if (target is IDamageable damageable)
                    {
                        ApplyDamage(damageable);
                    }
                    GD.Print("Hit target " + target.Name);
                    origin = hit + direction * 0.1f;
                }
            }

            if (_lastRaycastResult.ContainsKey("position"))
            {
                var collider = _lastRaycastResult["collider"];
                Node target = (Node)collider;
                Vector3 point = (Vector3)_lastRaycastResult["position"];
                _hitParticles.GlobalPosition = point;
                _hitParticles.Emitting = true;
                if (target is IDamageable damageable)
                {
                    ApplyDamage(damageable);
                }
            }
        }

        private Dictionary Raycast(Vector3 start, Vector3 direction)
        {
            var spaceState = GetWorld3D().DirectSpaceState;
            Vector3 end = start + direction.Normalized() * 1000f;
            var query = PhysicsRayQueryParameters3D.Create(start, end);
            query.CollideWithAreas = true;
            var result = spaceState.IntersectRay(query);
            return result;
        }

        private Dictionary RayCastForward()
        {
            var spaceState = GetWorld3D().DirectSpaceState;
            Vector3 start = _muzzle.GlobalPosition;
            Vector3 direction = -_muzzle.GlobalBasis.Z;
            Vector3 end = start + direction.Normalized() * 1000f;
            var query = PhysicsRayQueryParameters3D.Create(start, end);
            query.CollideWithAreas = true;
            var result = spaceState.IntersectRay(query);
            return result;
        }

        private void DoRayCast()
        {
            var spaceState = GetWorld3D().DirectSpaceState;
            Vector3 start = _muzzle.GlobalPosition;
            Vector3 direction = -_muzzle.GlobalBasis.Z;
            Vector3 end = start + direction.Normalized() * 1000f;
            var query = PhysicsRayQueryParameters3D.Create(start, end);
            query.CollideWithAreas = true;
            var result = spaceState.IntersectRay(query);

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

        private void DisableEffect()
        {
            _bulletEffect.Visible = false;
            _bulletTravel.Visible = false;
        }

        private void DrawBulletEffect()
        {
            _bulletEffect.Visible = true;
            _bulletTravel.Visible = true;
            Tween railgunTween = CreateTween();
            Material material = _bulletEffect.GetActiveMaterial(0);
            CylinderMesh mesh = (CylinderMesh)_bulletEffect.Mesh;
            float effectLength = mesh.Height;
            Vector3 startPosition = _bulletEffect.Position;
            startPosition.Z += effectLength / 2;
            Vector3 endPosition = startPosition;
            endPosition.Z -= effectLength;

            railgunTween.TweenProperty(material, "emission_energy_multiplier", 32f, 0.05f);
            railgunTween.Parallel().TweenProperty(mesh, "bottom_radius", 0.01f, 0.05f);
            railgunTween.Parallel().TweenProperty(mesh, "top_radius", 0.01f, 0.05f);
            railgunTween.Parallel().TweenProperty(_bulletTravel, "position", endPosition, 0.1f);
            railgunTween.TweenProperty(mesh, "bottom_radius", 0.05f, 0.15f);
            railgunTween.Parallel().TweenProperty(mesh, "top_radius", 0.05f, 0.15f);
            railgunTween.Parallel().TweenProperty(material, "emission_energy_multiplier", 0f, 0.15f);
            railgunTween.TweenCallback(Callable.From(DisableEffect));
            railgunTween.TweenProperty(_bulletTravel, "position", startPosition, 0.05f);
        }

        private void ApplyDamage(IDamageable damageable)
        {
            damageable.TakeDamage(_damageData);
        }
    }
}
