// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>

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

        private HashSet<Rocket> _rockets;

        public override void _Ready()
        {
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
            CallDeferred(MethodName.InitializeRockets);
        }

        public override void Attack()
        {
            if (CanAttack)
            {
                CanAttack = false;
                // Not awaiting for async completion on purpose.
                LaunchRockets();
            }
        }

        private void InitializeRockets()
        {
            _rockets = new HashSet<Rocket>();
            for (int i = 0; i < _salvoSize; i++)
            {
                Rocket rocket = _rocketScene.Instantiate<Rocket>();
                LevelManager.Active.AddLevelObject(rocket);
                _rockets.Add(rocket);
            }
        }

        private async Task LaunchRockets()
        {
            int shotCounter = 0;
            int launchPointIndex = 0;
            while (shotCounter < _salvoSize)
            {
                var rocket = FindNextAvailableRocket();
                if (rocket == null)
                {
                    GD.PushError(
                        "Rocket launcher did not have available rocket when one was expected. \n"
                            + "Adding new rocket to pool. Please report this error. "
                    );
                    return;
                }
                Node3D point = _launchPoints[launchPointIndex];
                launchPointIndex = (launchPointIndex + 1) % _launchPoints.Count;
                rocket.LaunchRocket(point, Vector3.Forward);
                shotCounter++;
                _intervalTimer.Start();
                await (ToSignal(_intervalTimer, "timeout"));
            }

            _cooldownTimer.Start();
        }

        private void OnCooldownTimerTimeout()
        {
            CanAttack = true;
        }

        private Rocket FindNextAvailableRocket()
        {
            foreach (Rocket rocket in _rockets)
            {
                if (rocket.IsAvailable)
                {
                    return rocket;
                }
            }

            return null;
        }
    }
}
