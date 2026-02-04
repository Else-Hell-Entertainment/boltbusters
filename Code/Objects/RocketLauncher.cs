using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Prototype Rocket Launcher. WIP.
    /// </summary>
    public partial class RocketLauncher : BaseWeapon
    {
        [Export]
        private PackedScene _rocketScene;

        [Export]
        private int _salvoSize = 8;

        [Export]
        private float _launchInterval = 0.1f;

        [Export]
        private float _cooldown = 5f;

        private Timer _cooldownTimer;
        private Timer _intervalTimer;

        private List<Node3D> _launchPoints = new List<Node3D>();
        private Node _levelRootNode;
        private bool _isFiring = false;
        private bool _canFire = true;

        public override void _Ready()
        {
            _levelRootNode = GetTree().CurrentScene;
            Node3D points = GetNode<Node3D>("LaunchPoints");
            foreach (var point in points.GetChildren())
            {
                if (point is Node3D)
                {
                    _launchPoints.Add((Node3D)point);
                }
            }

            _intervalTimer = GetNode<Timer>("IntervalTimer");
            _intervalTimer.WaitTime = _launchInterval;
            _intervalTimer.OneShot = true;
            _intervalTimer.Timeout += OnIntervalTimerTimeout;

            _cooldownTimer = GetNode<Timer>("CooldownTimer");
            _cooldownTimer.WaitTime = _cooldown;
            _cooldownTimer.OneShot = true;
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
        }

        public override void Attack()
        {
            LaunchRockets();
            //rocket.GlobalPosition = _launchPoint.GlobalPosition;
            //rocket.GlobalRotation = _launchPoint.GlobalRotation;
        }

        public override bool CanAttack()
        {
            return _canFire;
        }

        private async Task LaunchRockets()
        {
            GD.Print("Launching Rockets");
            int shotCounter = 0;
            _isFiring = true;
            while (shotCounter < _salvoSize)
            {
                Rocket rocket = _rocketScene.Instantiate<Rocket>();
                _levelRootNode.AddChild(rocket);
                Node3D point = _launchPoints[0];
                rocket.GlobalPosition = point.GlobalPosition;
                rocket.GlobalRotation = point.GlobalRotation;
                GD.Print("ROCKET GOES SKÄBÄDÄBÄDÄBÄDÄÄÄ!");
                shotCounter++;
                GD.Print("Shots fired: " + shotCounter);
                _intervalTimer.Start();
                await (ToSignal(_intervalTimer, "timeout"));
            }
        }

        private void OnCooldownTimerTimeout()
        {
            _canFire = true;
        }

        private void OnIntervalTimerTimeout() { }
    }
}
