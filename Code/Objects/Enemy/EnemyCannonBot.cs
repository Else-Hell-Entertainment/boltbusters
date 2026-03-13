using Godot;

namespace EHE.BoltBusters
{
    public partial class EnemyCannonBot : Enemy
    {
        [Export]
        private PackedScene _cannonBallScene;

        [Export]
        private float _range = 16f;

        [Export]
        private float _reloadTime = 5;

        [Export]
        private RayCast3D _rayCaster;

        [Export]
        public EntityController Controller { get; private set; }

        private CharacterBody3D _player;
        private Timer _reloadTimer;
        private bool _canFire = true;
        private Node3D _muzzle;

        private double _repathTimer = 0.25;
        private double _repathInterval = 0.5;

        public override void _Ready()
        {
            // Add small variance to how often the bots call the nav API for repathing so that they don't all query
            // at the exact same frame.
            _repathInterval += GD.RandRange(0.0, 0.1);
            _player = TargetProvider.Instance.Player;
            _reloadTimer = GetNode<Timer>("ReloadTimer");
            _reloadTimer.Timeout += OnReloadTimerTimeout;
            _reloadTimer.OneShot = true;
            _muzzle = GetNode<Node3D>("Turret/Muzzle");
        }

        public override void _Process(double delta)
        {
            if (_repathTimer < _repathInterval)
            {
                _repathTimer += delta;
            }
            else
            {
                _repathTimer = 0;
                if (IsInstanceValid(_player))
                {
                    Controller.AddCommand(new MoveToPositionCommand(_player.GlobalPosition));
                }
            }
            if (IsInstanceValid(_player))
            {
                Controller.AddCommand(new RotateTowardsCommand(_player.GlobalPosition));
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_canFire && IsInstanceValid(_player) && IsPlayerInLineOfSight())
            {
                Attack();
            }
        }

        private void Attack()
        {
            _canFire = false;
            _reloadTimer.Start();
            CannonBall ball = _cannonBallScene.Instantiate<CannonBall>();
            LevelManager.Active.AddLevelObject(ball);
            ball.GlobalPosition = _muzzle.GlobalPosition;
            ball.GlobalRotation = _muzzle.GlobalRotation;
        }

        /// <summary>
        /// Uses RaycastNode3D to check if the player is directly in front of the cannon and can be hit.
        /// </summary>
        /// <returns>True if there is no obstruction between cannon and player.</returns>
        private bool IsPlayerInLineOfSight()
        {
            return _rayCaster.IsColliding() && _rayCaster.GetCollider() is Player;
        }

        private void OnReloadTimerTimeout()
        {
            _canFire = true;
        }

        public override void OnSpawn() { }
    }
}
