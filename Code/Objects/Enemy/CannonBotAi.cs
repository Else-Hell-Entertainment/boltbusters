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

        public override void _Ready()
        {
            base._Ready();
            _player = TargetProvider.Instance.Player;
        }

        public override void _PhysicsProcess(double delta)
        {
            SurroundPlayer();
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
            Vector3 _point1 = _player.GlobalPosition + new Vector3(0, 0, distance);
            Vector3 _point2 = _player.GlobalPosition + new Vector3(0, 0, -distance);
            Vector3 _point3 = _player.GlobalPosition + new Vector3(distance, 0, 0);
            Vector3 _point4 = _player.GlobalPosition + new Vector3(-distance, 0, 0);

            MoveTowardsPoint(_bot1, _point1);
            FacePlayer(_bot1);
            MoveTowardsPoint(_bot2, _point2);
            FacePlayer(_bot2);
            MoveTowardsPoint(_bot3, _point3);
            FacePlayer(_bot3);
            MoveTowardsPoint(_bot4, _point4);
            FacePlayer(_bot4);
        }
    }
}
