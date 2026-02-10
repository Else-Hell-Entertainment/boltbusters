using Godot;

namespace EHE.BoltBusters
{
    public partial class CannonBotAi : Node3D
    {
        [Export]
        private EnemyCannon _bot1;

        [Export]
        private EnemyCannon _bot2;

        [Export]
        private EnemyCannon _bot3;

        [Export]
        private EnemyCannon _bot4;

        private CharacterBody3D _player;

        private double _timer;
        private double _switchTime = 10;
        private bool _isSurrounding = true;

        public override void _Ready()
        {
            base._Ready();
            _player = TargetProvider.Instance.Player;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isSurrounding)
            {
                SurroundPlayer();
            }
            else
            {
                DiamondFormation();
            }

            _timer += delta;
            if (_timer >= _switchTime)
            {
                _timer = 0;
                if (_isSurrounding)
                {
                    _isSurrounding = false;
                }
                else
                {
                    _isSurrounding = true;
                }
            }
        }

        private void MoveTowardsPlayer(EnemyCannon bot)
        {
            Vector3 direction = (_player.Position - bot.Position).Normalized();
            bot.Controller.AddCommand(new MoveToDirectionCommand(direction));
        }

        private void MoveTowardsPoint(EnemyCannon bot, Vector3 point)
        {
            Vector3 direction = (point - bot.GlobalPosition);
            if (direction.Length() < 0.2f)
            {
                return;
            }
            bot.Controller.AddCommand(new MoveToDirectionCommand(direction));
        }

        private void FacePlayer(EnemyCannon bot)
        {
            bot.Controller.AddCommand(new RotateTowardsCommand(_player.GlobalPosition));
        }

        private void SurroundPlayer()
        {
            float distance = 7;
            Vector3 point1 = _player.GlobalPosition + new Vector3(0, 0, distance);
            Vector3 point2 = _player.GlobalPosition + new Vector3(0, 0, -distance);
            Vector3 point3 = _player.GlobalPosition + new Vector3(distance, 0, 0);
            Vector3 point4 = _player.GlobalPosition + new Vector3(-distance, 0, 0);

            MoveTowardsPoint(_bot1, point1);
            FacePlayer(_bot1);
            MoveTowardsPoint(_bot2, point2);
            FacePlayer(_bot2);
            MoveTowardsPoint(_bot3, point3);
            FacePlayer(_bot3);
            MoveTowardsPoint(_bot4, point4);
            FacePlayer(_bot4);
        }

        private void DiamondFormation()
        {
            float distance = 5;
            Vector3 point1 = _player.GlobalPosition + new Vector3(0, 0, -distance);
            Vector3 point2 = point1 + new Vector3(0, 0, -distance * 2);
            Vector3 point3 = point1 + new Vector3(-distance, 0, -distance);
            Vector3 point4 = point1 + new Vector3(distance, 0, -distance);
            MoveTowardsPoint(_bot1, point1);
            FacePlayer(_bot1);
            MoveTowardsPoint(_bot2, point2);
            FacePlayer(_bot2);
            MoveTowardsPoint(_bot3, point3);
            FacePlayer(_bot3);
            MoveTowardsPoint(_bot4, point4);
            FacePlayer(_bot4);
        }
    }
}
