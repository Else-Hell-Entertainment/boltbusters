using System;
using Godot;

namespace EHE.BoltBusters
{
    public partial class CannonBall : Node3D
    {
        [Export]
        private float _speed = 20.0f;

        private float _lifeTime = 10.0f;
        private Timer _despawnTimer;
        private Area3D _hitArea;

        public override void _Ready()
        {
            _hitArea = GetNode<Area3D>("HitArea");
            _hitArea.BodyEntered += OnBodyEntered;
            _despawnTimer = GetNode<Timer>("DespawnTimer");
            _despawnTimer.WaitTime = _lifeTime;
            _despawnTimer.OneShot = true;
            _despawnTimer.Timeout += OnDespawnTimerTimeout;
            _despawnTimer.Start();
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector3 dir = -Transform.Basis.Z;
            Position += dir * _speed * (float)delta;
        }

        private void OnDespawnTimerTimeout()
        {
            QueueFree();
            GD.Print("CANNON BALL DESTROYED");
        }

        private void OnBodyEntered(Node3D body)
        {
            if (body is Player)
            {
                GD.Print("CANNON BALL HIT PLAYER IN THE HEEEEEEEAAAADDDD");
                QueueFree();
            }
        }
    }
}
