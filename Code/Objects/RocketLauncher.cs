using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Rocket launcher of type BaseWeapon with multiple launch points. Fires a full salvo of rockets in sequence by
    /// cycling through the launch points. IMPORTANT: Set the rocket scene in editor for the rocket.
    /// </summary>
    public partial class RocketLauncher : BaseWeapon
    {
        [Export]
        private PackedScene _rocketScene;

        [Export]
        private int _salvoSize = 4;

        // Interval between rocket launches within a salvo.
        [Export]
        private float _launchInterval = 0.2f;

        // Cooldown between salvo launches. Starts only after the entire salvo is completed.
        [Export]
        private float _cooldown = 5f;

        private Timer _cooldownTimer;
        private Timer _intervalTimer;
        private List<Node3D> _launchPoints = new List<Node3D>();

        // Rockets will be reparented under this node.
        private Node _levelRootNode;
        private bool _canFire = true;

        public override void _Ready()
        {
            _levelRootNode = GetTree().CurrentScene;
            Node3D points = GetNode<Node3D>("LaunchPoints");
            foreach (var point in points.GetChildren())
            {
                if (point is Node3D node3D)
                {
                    _launchPoints.Add(node3D);
                }
            }

            if (_launchPoints.Count == 0)
            {
                GD.PrintErr("Rocket launcher missing launch points!");
            }

            _intervalTimer = GetNode<Timer>("IntervalTimer");
            _intervalTimer.WaitTime = _launchInterval;
            _intervalTimer.OneShot = true;

            _cooldownTimer = GetNode<Timer>("CooldownTimer");
            _cooldownTimer.WaitTime = _cooldown;
            _cooldownTimer.OneShot = true;
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
        }

        public override void Attack()
        {
            if (CanAttack())
            {
                _canFire = false;
                // Not awaiting for async completion on purpose.
                LaunchRockets();
            }
        }

        public override bool CanAttack()
        {
            return _canFire;
        }

        private async Task LaunchRockets()
        {
            int shotCounter = 0;
            int launchPointIndex = 0;
            while (shotCounter < _salvoSize)
            {
                Rocket rocket = _rocketScene.Instantiate<Rocket>();
                _levelRootNode.AddChild(rocket);
                Node3D point = _launchPoints[launchPointIndex];
                launchPointIndex = (launchPointIndex + 1) % _launchPoints.Count;
                rocket.GlobalPosition = point.GlobalPosition;
                rocket.GlobalRotation = point.GlobalRotation;
                shotCounter++;
                _intervalTimer.Start();
                await (ToSignal(_intervalTimer, "timeout"));
            }
            _cooldownTimer.Start();
        }

        private void OnCooldownTimerTimeout()
        {
            _canFire = true;
        }
    }
}
