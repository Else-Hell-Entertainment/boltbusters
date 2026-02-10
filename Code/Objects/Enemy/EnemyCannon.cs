using Godot;

namespace EHE.BoltBusters
{
    public partial class EnemyCannon : Character
    {
        [Export]
        private PackedScene _cannonBallScene;

        [Export]
        private float _range = 16f;

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
            if (IsPlayerInAttackCone() && _canFire)
            {
                Attack();
            }
        }

        private void Attack()
        {
            _canFire = false;
            _reloadTimer.Start();
            CannonBall ball = _cannonBallScene.Instantiate<CannonBall>();
            GetTree().GetRoot().AddChild(ball);
            ball.GlobalPosition = _muzzle.GlobalPosition;
            ball.GlobalRotation = _muzzle.GlobalRotation;
        }

        /// <summary>
        /// Checks if the player is within the accepted attack range and directly in front of the cannon. Method allows
        /// for 0.02 rad (a little over 1 degree) deviation in angle to account for minor errors.
        /// </summary>
        /// <returns></returns>
        private bool IsPlayerInAttackCone()
        {
            float angleTolerance = 0.02f;
            Vector3 direction = _player.GlobalPosition - _muzzle.GlobalPosition;
            direction.Y = 0;
            Vector3 origin = -_muzzle.GlobalBasis.Z;
            origin.Y = 0;
            float angle = origin.AngleTo(direction);

            return angle < angleTolerance && direction.Length() < _range;
        }

        private void OnReloadTimerTimeout()
        {
            _canFire = true;
        }
    }
}
