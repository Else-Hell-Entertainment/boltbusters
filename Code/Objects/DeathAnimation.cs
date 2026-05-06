// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

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
        private float _weakDeathStrength = 1.0f;

        [Export]
        private float _mediumDeathStrength = 3.0f;

        [Export]
        private float _hardDeathStrength = 10.0f;

        [Export]
        private float _randomness = 1.0f;

        private float _effectStrength = 1.0f;

        private Vector3 dir;

        public DamageType DamageType;

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

            switch (DamageType)
            {
                case DamageType.Chaingun:
                    _effectStrength = _weakDeathStrength;
                    break;
                case DamageType.Missile:
                    _effectStrength = _mediumDeathStrength;
                    break;
                case DamageType.Sniper:
                    _effectStrength = _hardDeathStrength;
                    break;
            }

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
                part.ApplyImpulse(dir * _effectStrength * (float)GD.RandRange(0.3f, 1.0f) * _randomness);
            }
        }

        private void Despawn()
        {
            QueueFree();
        }
    }
}
