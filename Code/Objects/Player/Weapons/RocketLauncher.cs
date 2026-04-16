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
        private int _baseSalvoSize = 4;

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

        private int _shotCounter = 0;
        private int _launchPointIndex = 0;
        private bool _launchInProgress;

        public int SalvoSizeUpgrades { get; private set; } = 0;

        public enum LauncherState
        {
            None = 0,
            ReadyToFire,
            NotReadyToFire,
            LaunchingRockets,
            RocketJustLaunched,
            ReloadingStarted,
            ReloadingFinished,
        }

        [Signal]
        public delegate void RocketLauncherStateChangedEventHandler(int state);

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
            _intervalTimer.Timeout += OnIntervalTimerTimeout;

            _cooldownTimer = GetNode<Timer>("CooldownTimer");
            _cooldownTimer.WaitTime = _cooldown;
            _cooldownTimer.OneShot = true;
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
            CallDeferred(MethodName.InitializeRockets);

            EmitSignal(SignalName.RocketLauncherStateChanged, (int)LauncherState.ReadyToFire);
        }

        public override void Attack()
        {
            if (CanAttack)
            {
                CanAttack = false;
                StartLaunching();
                // Not awaiting for async completion here on purpose.
                //LaunchRockets();
            }
        }

        public override void Reset()
        {
            _cooldownTimer.Stop();
            _launchInProgress = false;
            OnCooldownTimerTimeout();
        }

        /// <summary>
        /// Increases the SalvoSizeUpgrade count by one and adds a new rocket to the rocket pool.
        /// </summary>
        public void IncreaseSalvoSize()
        {
            SalvoSizeUpgrades++;
            AddNewRocket();
        }

        /// <summary>
        /// Removes one upgrade from salvo size. The count can never go below the base value set in code. Will not
        /// remove the corresponding rocket from the rocket pool.
        /// </summary>
        public void DecreaseSalvoSize()
        {
            if (SalvoSizeUpgrades > 0)
            {
                SalvoSizeUpgrades--;
            }
            else
            {
                GD.PrintErr("Attempting to remove non-existing salvo size upgrade from rocket launcher " + this);
            }
        }

        private void InitializeRockets()
        {
            _rockets = new HashSet<Rocket>();
            for (int i = 0; i < _baseSalvoSize; i++)
            {
                AddNewRocket();
            }
        }

        private void AddNewRocket()
        {
            Rocket rocket = _rocketScene.Instantiate<Rocket>();
            LevelManager.Active.AddLevelObject(rocket);
            _rockets.Add(rocket);
        }

        /*
        private async Task LaunchRockets()
        {
            int shotCounter = 0;
            int launchPointIndex = 0;
            while (shotCounter < _baseSalvoSize + SalvoSizeUpgrades)
            {
                _launchInProgress = true;
                var rocket = FindNextAvailableRocket();
                if (rocket == null)
                {
                    GD.PushError(
                        "Rocket launcher did not have available rocket when one was expected. \n"
                            + "Adding new rocket to pool. Please report this error. "
                    );
                    AddNewRocket();
                    return;
                }
                Node3D point = _launchPoints[launchPointIndex];
                launchPointIndex = (launchPointIndex + 1) % _launchPoints.Count;
                rocket.LaunchRocket(point, Vector3.Forward);
                shotCounter++;
                _intervalTimer.Start();
                EmitSignal(SignalName.RocketLauncherStateChanged, (int)LauncherState.NotReadyToFire);
                EmitSignal(SignalName.RocketLauncherStateChanged, (int)LauncherState.RocketJustLaunched);
                await (ToSignal(_intervalTimer, "timeout"));
            }
            _launchInProgress = false;

            EmitSignal(SignalName.RocketLauncherStateChanged, (int)LauncherState.ReloadingStarted);
            _cooldownTimer.Start();
        }
        */

        private void StartLaunching()
        {
            _launchInProgress = true;
            CanAttack = false;
            LaunchRocket();
        }

        private void LaunchRocket()
        {
            // This is used when launching is in progress, but weapon state is reset suddenly (for example round ends).
            if (!_launchInProgress)
            {
                return;
            }
            if (_shotCounter < _baseSalvoSize + SalvoSizeUpgrades)
            {
                var rocket = FindNextAvailableRocket();
                if (rocket == null)
                {
                    GD.PushError(
                        "Rocket launcher did not have available rocket when one was expected. \n"
                            + "Adding new rocket to pool. Please report this error. "
                    );
                    AddNewRocket();
                    _intervalTimer.Start();
                    return;
                }
                Node3D point = _launchPoints[_launchPointIndex];
                _launchPointIndex = (_launchPointIndex + 1) % _launchPoints.Count;
                rocket.LaunchRocket(point, Vector3.Forward);
                _shotCounter++;
                _intervalTimer.Start();
                EmitSignal(SignalName.RocketLauncherStateChanged, (int)LauncherState.NotReadyToFire);
                EmitSignal(SignalName.RocketLauncherStateChanged, (int)LauncherState.RocketJustLaunched);
            }
            else
            {
                _launchInProgress = false;
                _shotCounter = 0;
                EmitSignal(SignalName.RocketLauncherStateChanged, (int)LauncherState.ReloadingStarted);
                _cooldownTimer.Start();
            }
        }

        private void OnIntervalTimerTimeout()
        {
            LaunchRocket();
        }

        private void OnCooldownTimerTimeout()
        {
            CanAttack = true;
            EmitSignal(SignalName.RocketLauncherStateChanged, (int)LauncherState.ReadyToFire);
            EmitSignal(SignalName.RocketLauncherStateChanged, (int)LauncherState.ReloadingFinished);
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
