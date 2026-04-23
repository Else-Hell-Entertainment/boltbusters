// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters
{
    public partial class BossMeleeWeapon : Node3D
    {
        [ExportGroup("Weapon Stats")]
        [Export]
        private float _meleeCooldown = 1.0f;

        [Export]
        private int _meleeDamage;

        [ExportGroup("Node References")]
        [Export]
        private Enemy _meleeOwner;

        [Export]
        private Area3D _triggerArea;

        [Export]
        private Area3D _attackArea;

        [Export]
        private Timer _cooldownTimer;

        [Export]
        private AnimationPlayer _animationPlayer;

        private const int COLLISION_MASK_PLAYER = 1;
        private const string ATTACK_ANIMATION_NAME = "Attack";

        private DamageData _damageData;

        private bool _isAttacking;
        private bool _attackOnCooldown;
        private CharacterBody3D _player;

        public override void _Ready()
        {
            base._Ready();
            _cooldownTimer.WaitTime = _meleeCooldown;
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
            _damageData = new DamageData(_meleeDamage, DamageType.Melee);
            _triggerArea.BodyEntered += OnTriggerAreaEntered;
            _triggerArea.CollisionMask = COLLISION_MASK_PLAYER;
            _attackArea.BodyEntered += OnAttackAreaEntered;
            _player = LevelManager.Active.Player;
        }

        public void SetAttackState(bool isAttacking)
        {
            _isAttacking = isAttacking;
            _attackArea.SetCollisionMaskValue(COLLISION_MASK_PLAYER, isAttacking);
        }

        private void TriggerAttack()
        {
            _animationPlayer.Play(ATTACK_ANIMATION_NAME);
            _cooldownTimer.Start();
            _attackOnCooldown = true;
            _meleeOwner.SetMoveSpeed(_meleeOwner.AfterAttackSpeed);
        }

        private void OnTriggerAreaEntered(Node3D body)
        {
            if (body is Player)
            {
                TriggerAttack();
            }
        }

        private void OnAttackAreaEntered(Node3D body)
        {
            if (body is Player player)
            {
                player.TakeDamage(_damageData);
            }
        }

        private void OnCooldownTimerTimeout()
        {
            _attackOnCooldown = false;
            CheckTriggerArea();
            _meleeOwner.SetMoveSpeed(_meleeOwner.NormalSpeed);
        }

        private void CheckTriggerArea()
        {
            if (_triggerArea.OverlapsBody(_player))
            {
                TriggerAttack();
            }
        }
    }
}
