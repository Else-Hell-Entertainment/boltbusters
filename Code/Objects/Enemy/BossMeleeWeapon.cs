using System;
using Godot;

namespace EHE.BoltBusters
{
    public partial class BossMeleeWeapon : Node3D
    {
        [ExportGroup("Weapon Stats")]
        [Export]
        private float _meleeCooldown;

        [Export]
        private int _meleeDamage;

        [ExportGroup("Node References")]
        [Export]
        private Area3D _triggerArea;

        [Export]
        private Area3D _attackArea;

        [Export]
        private Timer _cooldownTimer;

        [Export]
        private AnimationPlayer _animationPlayer;

        private DamageData _damageData;

        private bool _isAttacking;

        public override void _Ready()
        {
            base._Ready();
            _cooldownTimer.WaitTime = _meleeCooldown;
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
            _damageData = new DamageData(_meleeDamage, DamageType.Melee);
        }

        private void OnCooldownTimerTimeout()
        {
            throw new NotImplementedException();
        }
    }
}
