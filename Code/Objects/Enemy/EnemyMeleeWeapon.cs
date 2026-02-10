using Godot;

namespace EHE.BoltBusters
{
    public partial class EnemyMeleeWeapon : BaseWeapon
    {
        [Export]
        private float _attackCooldown = 5.0f;

        private Area3D _attackArea;
        private Timer _cooldownTimer;
        private GpuParticles3D _hitParticles;
        private DamageData _damageData;

        public override void _Ready()
        {
            InitializeNodes();
            _damageData = new DamageData(5, DamageType.Melee);
        }

        public override void Attack()
        {
            CallDeferred(nameof(CheckAttackArea));
        }

        public override void _PhysicsProcess(double delta)
        {
            if (CanAttack)
            {
                Attack();
            }
        }

        private void CheckAttackArea()
        {
            var bodies = _attackArea.GetOverlappingBodies();

            foreach (var body in bodies)
            {
                if (body is IDamageable targetBody and Player)
                {
                    targetBody.TakeDamage(_damageData);
                    _hitParticles.Emitting = true;
                    CanAttack = false;
                    _cooldownTimer.Start();
                }
            }
        }

        private void InitializeNodes()
        {
            _cooldownTimer = GetNode<Timer>("CooldownTimer");
            _attackArea = GetNode<Area3D>("AttackArea");
            _hitParticles = GetNode<GpuParticles3D>("HitParticles");

            if (_cooldownTimer == null || _attackArea == null || _hitParticles == null)
            {
                GD.PrintErr("Some of EnemyMeleeWeapon nodes not found during init. Node is borken.");
                return;
            }

            _cooldownTimer.WaitTime = _attackCooldown;
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
        }

        private void OnCooldownTimerTimeout()
        {
            CanAttack = true;
        }
    }
}
