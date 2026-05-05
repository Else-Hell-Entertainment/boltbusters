// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>

using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Represents a melee weapon for enemies that attacks the player when in range.
    /// Uses an Area3D to detect the player and plays attack animations on hit.
    /// </summary>
    public partial class EnemyMeleeWeapon : BaseWeapon
    {
        /// <summary>
        /// Time in seconds between consecutive attacks.
        /// </summary>
        [Export]
        private float _attackCooldown = 5.0f;

        /// <summary>
        /// Amount of damage dealt to the player per attack.
        /// </summary>
        [Export]
        private int _attackDamage = 5;

        /// <summary>
        /// Where this weapon is connected to.
        /// </summary>
        [Export]
        private Enemy _meleeOwner;

        /// <summary>
        /// Animation player for playing attack and idle animations.
        /// </summary>
        private AnimationPlayer _animationPlayer;

        /// <summary>
        /// Area used to detect if the player is within attack range.
        /// </summary>
        private Area3D _attackArea;

        /// <summary>
        /// Timer that controls the cooldown between attacks.
        /// </summary>
        private Timer _cooldownTimer;

        /// <summary>
        /// Particle effect displayed when the attack hits the player.
        /// </summary>
        private GpuParticles3D _hitParticles;

        /// <summary>
        /// Cached damage data passed to the player when attacked.
        /// </summary>
        private DamageData _damageData;

        /// <summary>
        /// Tracks whether the enemy is currently in attack mode (player is in range).
        /// </summary>
        private bool _isAttacking;

        /// <summary>
        /// Animation name for the attack animation. Currently hardcoded here, but location is to be changed.
        /// </summary>
        private const string ATTACK_ANIMATION_NAME = "HammerBotAnimations/Attack";

        /// <summary>
        /// Animation name for the idle animation. Currently hardcoded here, but location is to be changed.
        /// </summary>
        private const string IDLE_ANIMATION_NAME = "HammerBotAnimations/Idle";

        public override void _Ready()
        {
            InitializeNodes();
            ConnectSignals();
            _damageData = new DamageData(_attackDamage, DamageType.Melee);
        }

        /// <summary>
        /// Executes an attack by checking if the player is in the attack area.
        /// Called using CallDeferred to avoid physics state issues.
        /// </summary>
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

        /// <summary>
        /// Checks if the player is within the attack area and executes the attack.
        /// Deals damage, plays hit particles, starts cooldown, and triggers attack animation.
        /// Sets _isAttacking to false if the player leaves the area.
        /// </summary>
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
                    _animationPlayer.Play(ATTACK_ANIMATION_NAME);

                    _meleeOwner.SetMoveSpeed(_meleeOwner.AfterAttackSpeed);
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
                this.LogError("Some of EnemyMeleeWeapon nodes not found during init. Node is broken.");
                return;
            }
            _cooldownTimer.WaitTime = _attackCooldown;
        }

        /// <summary>
        /// Connects all signal handlers for timers, areas, and animation events.
        /// </summary>
        private void ConnectSignals()
        {
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
            _attackArea.BodyEntered += OnAttackAreaBodyEntered;
            _animationPlayer.AnimationFinished += OnAnimationFinished;
        }

        /// <summary>
        /// Called when the cooldown timer expires.
        /// Re-enables the weapon's ability to attack.
        /// </summary>
        private void OnCooldownTimerTimeout()
        {
            CanAttack = true;
            _meleeOwner.SetMoveSpeed(_meleeOwner.NormalSpeed);
        }

        /// <summary>
        /// Called when a body enters the attack area.
        /// If the body is the player, enables attack mode.
        /// </summary>
        /// <param name="body">The body that entered the attack area.</param>
        private void OnAttackAreaBodyEntered(Node3D body)
        {
            if (body is Player)
            {
                _isAttacking = true;
            }
        }

        private void OnAnimationFinished(StringName animationName)
        {
            if (animationName == ATTACK_ANIMATION_NAME)
            {
                _animationPlayer.Play(IDLE_ANIMATION_NAME);
            }
        }
    }
}
