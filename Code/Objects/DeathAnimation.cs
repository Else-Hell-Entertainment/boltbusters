using System;
using Godot;

namespace EHE.BoltBusters
{
    public partial class DeathAnimation : Projectile
    {
        [Export]
        private AudioStreamPlayer3D _deathSound;

        [Export]
        private RigidBody3D[] _deathParts;

        [Export]
        private Timer _despawnTimer;

        [Export]
        public float EffectStrength = 1.0f;

        [Export]
        private float _randomness = 1.0f;

        private Vector3 dir;

        public override void _Ready()
        {
            if (_despawnTimer != null)
            {
                _despawnTimer.Timeout += Despawn;
            }
        }

        public void PlayDeathAnimation(Vector3 direction)
        {
            _despawnTimer.Start();
            dir = direction;
            CallDeferred(MethodName.ApplyImpulse);
            if (_deathSound != null)
            {
                _deathSound.Play();
            }
        }

        private void ApplyImpulse()
        {
            for (int i = 0; i < _deathParts.Length; i++)
            {
                RigidBody3D part = _deathParts[i];
                part.ApplyImpulse(dir * EffectStrength * GD.Randf() * _randomness);
            }
        }

        private void Despawn()
        {
            QueueFree();
        }
    }
}
