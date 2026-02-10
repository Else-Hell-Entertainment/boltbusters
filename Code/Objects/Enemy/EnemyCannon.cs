using Godot;

namespace EHE.BoltBusters
{
    public partial class EnemyCannon : Character
    {
        [Export]
        private PackedScene _cannonBallScene;

        [Export]
        private float _range = 20;

        [Export]
        private float _reloadTime = 5;

        [Export]
        public EntityController Controller { get; private set; }
        private CharacterBody3D _player;
        private Timer _reloadTimer;
        private bool _canFire = true;
        private Node3D _muzzle;

        public override void _Ready()
        {
            _player = TargetProvider.Instance.Player;
            _reloadTimer = GetNode<Timer>("ReloadTimer");
            _reloadTimer.Timeout += OnReloadTimerTimeout;
            _reloadTimer.OneShot = true;
            _muzzle = GetNode<Node3D>("Turret/Muzzle");
        }

        public override void _PhysicsProcess(double delta)
        {
            //FacePlayer();
            Attack();
        }

        private void Attack()
        {
            if (_canFire)
            {
                _canFire = false;
                _reloadTimer.Start();
                CannonBall ball = _cannonBallScene.Instantiate<CannonBall>();
                GetTree().GetRoot().AddChild(ball);
                ball.GlobalPosition = _muzzle.GlobalPosition;
                ball.GlobalRotation = _muzzle.GlobalRotation;
            }
        }

        private void FacePlayer()
        {
            if (_player == null)
                return;
            LookAt(_player.GlobalPosition);
        }

        private void OnReloadTimerTimeout()
        {
            _canFire = true;
        }
    }
}
