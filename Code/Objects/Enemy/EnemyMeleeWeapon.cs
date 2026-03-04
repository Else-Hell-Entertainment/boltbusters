// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters
{
    public partial class EnemyMeleeWeapon : BaseWeapon
    {
        [Export]
        private float _attackCooldown = 5.0f;

        private AnimationPlayer _animationPlayer;

        private Area3D _attackArea;
        private Timer _cooldownTimer;
        private GpuParticles3D _hitParticles;
        private DamageData _damageData;
        private bool _isAttacking;

        private const string AttackAnimationName = "HammerBotAnimations/Attack";
        private const string IdleAnimationName = "HammerBotAnimations/Idle";

        public override void _Ready()
        {
            InitializeNodes();
            ConnectSignals();
            _damageData = new DamageData(5, DamageType.Melee);
        }

        public override void Attack()
        {

            CallDeferred(nameof(CheckAttackArea));
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isAttacking && CanAttack)
            {
                Attack();
            }
        }

        private void CheckAttackArea()
        {
            var bodies = _attackArea.GetOverlappingBodies();
            bool isPlayerFound = false;

            foreach (var body in bodies)
            {
                // Execute the attack if player is in the attack area.
                if (body is IDamageable targetBody and Player)
                {
                    isPlayerFound = true;
                    targetBody.TakeDamage(_damageData);
                    _hitParticles.Emitting = true;
                    CanAttack = false;
                    _cooldownTimer.Start();
                    _animationPlayer.Play(AttackAnimationName);
                }
            }
            // Player has exited the attack area so enemy stops attacking.
            if (!isPlayerFound)
            {
                _isAttacking = false;
            }
        }

        private void InitializeNodes()
        {
            _cooldownTimer = GetNode<Timer>("CooldownTimer");
            _attackArea = GetNode<Area3D>("AttackArea");
            _hitParticles = GetNode<GpuParticles3D>("HitParticles");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            if (_cooldownTimer == null || _attackArea == null || _hitParticles == null || _animationPlayer == null)
            {
                GD.PrintErr("Some of EnemyMeleeWeapon nodes not found during init. Node is borken.");
                return;
            }
            _cooldownTimer.WaitTime = _attackCooldown;
        }

        private void ConnectSignals()
        {
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
            _attackArea.BodyEntered += OnAttackAreaBodyEntered;
            _animationPlayer.AnimationFinished += OnAnimationFinished;

        }

        private void OnCooldownTimerTimeout()
        {
            CanAttack = true;
        }

        private void OnAttackAreaBodyEntered(Node3D body)
        {
            if (body is Player)
            {
                _isAttacking = true;
            }

        }

        private void OnAnimationFinished(StringName animationName)
        {
            if (animationName == AttackAnimationName)
            {
                _animationPlayer.Play(IdleAnimationName);
            }

        }
    }
}
