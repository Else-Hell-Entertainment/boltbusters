// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters
{
    public partial class EnemyCannonBot : Enemy
    {
        [ExportGroup("Enemy stats")]
        [Export]
        private float _range = 16f;

        [Export]
        private float _projectileSpeed = 15f;

        [Export]
        private float _reloadTime = 5;

        [Export]
        private int _damage = 5;

        [Export]
        private float _moveSpeedSet = 5.0f;

        [Export]
        private float _afterAttackSpeedSet = 2.0f;

        [Export]
        private float _afterAttackSpeedTimer = 2.0f;

        [ExportGroup("References")]
        [Export]
        private RayCast3D _rayCaster;

        [Export]
        public EntityController Controller { get; private set; }

        [Export]
        private PackedScene _cannonBallScene;

        [Export]
        private AnimationPlayer _animationPlayer;

        [Export]
        private EnemyCannonController _cannonController;

        private CharacterBody3D _player;
        private Timer _reloadTimer;
        private Timer _slownessTimer;
        private bool _canFire = true;
        private Node3D _muzzle;

        private double _repathTimer = 0.25;
        private double _repathInterval = 0.3;

        private CannonBall _cannonBall;

        public override void _Ready()
        {
            // Add small variance to how often the bots call the nav API for repathing so that they don't all query
            // at the exact same frame.
            _repathInterval += GD.RandRange(0.0, 0.02);
            _player = TargetProvider.Instance.Player;

            _reloadTimer = GetNode<Timer>("ReloadTimer");
            _reloadTimer.Timeout += OnReloadTimerTimeout;
            _reloadTimer.OneShot = true;
            _reloadTimer.WaitTime = _reloadTime;

            _slownessTimer = GetNode<Timer>("SlownessTimer");
            _slownessTimer.Timeout += OnAfterAttackSpeedTimeout;
            _slownessTimer.OneShot = true;
            _slownessTimer.WaitTime = _afterAttackSpeedTimer;

            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _muzzle = GetNode<Node3D>("Turret/Muzzle");
            _cannonBall = _cannonBallScene.Instantiate<CannonBall>();
            _cannonBall.Initialize(_damage, _projectileSpeed);
            LevelManager.Active.AddLevelObject(_cannonBall);
            _cannonBall.Reset();

            // Initialize speed values
            _moveSpeed = _moveSpeedSet;
            _normalSpeed = _moveSpeedSet;
            _afterAttackSpeed = _afterAttackSpeedSet;
        }

        public override void _Process(double delta)
        {
            // Leaving old implementation here just in case.
            /*
            if (!IsInstanceValid(_player))
            {
                GD.Print("Player not found");
                return;
            }

            if (_repathTimer < _repathInterval)
            {
                _repathTimer += delta;
            }

            Vector3 invertedDirection = GlobalPosition - _player.GlobalPosition;
            float distanceToPlayer = invertedDirection.Length();
            float separation = _range - distanceToPlayer;

            if (Mathf.Abs(separation) < 0.1f)
            {
                Controller.AddCommand(new StopMovementCommand());
            }
            else if (_repathTimer > _repathInterval)
            {
                _repathTimer = 0;
                Vector3 targetPosition = _player.GlobalPosition;
                if (distanceToPlayer < _range)
                {
                    targetPosition += invertedDirection.Normalized() * _range;
                }
                Controller.AddCommand(new MoveToPositionCommand(targetPosition));
            }

            Controller.AddCommand(new RotateTowardsCommand(_player.GlobalPosition));
            */
        }

        public override void _PhysicsProcess(double delta)
        {
            // Autorotate towards player always unless overriden from somewhere else.
            if (LevelManager.Active.Player != null)
            {
                Controller.AddCommand(new RotateTowardsCommand(LevelManager.Active.Player.GlobalPosition));
            }

            if (_canFire && IsInstanceValid(_player) && IsPlayerInLineOfSight())
            {
                Attack();
            }
        }

        private void Attack()
        {
            _canFire = false;
            _reloadTimer.Start();
            _cannonBall.GlobalPosition = _muzzle.GlobalPosition;
            _cannonBall.GlobalRotation = _muzzle.GlobalRotation;
            _cannonBall.Activate();
            _animationPlayer.Play("CannonbotShoot");

            _cannonController.SetMoveSpeed(AfterAttackSpeed);
            _slownessTimer.Start();
            //CannonBall ball = _cannonBallScene.Instantiate<CannonBall>();
            //LevelManager.Active.AddLevelObject(ball);
            //ball.GlobalPosition = _muzzle.GlobalPosition;
            //ball.GlobalRotation = _muzzle.GlobalRotation;
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

        private void OnAfterAttackSpeedTimeout()
        {
            _cannonController.SetMoveSpeed(NormalSpeed);
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            _cannonBall.QueueFree();
        }
    }
}
