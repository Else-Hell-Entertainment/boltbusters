using Godot;

namespace EHE.BoltBusters.EnemyAI
{
    public partial class GroupBehaviourCannonbotDiamond : BaseGroupBehaviour
    {
        public override EnemyType AcceptedEnemyType => EnemyType.Ranged;

        public override int GroupSize => 4;

        private float _distanceToPlayer = 6;

        private Player _player;

        //TODO: Fetch dynamically. Hardcoded for testing purposes.
        private Vector3 _levelCenter = new Vector3(25, 0, 25);

        public override void _Ready()
        {
            base._Ready();
            _player = LevelManager.Active.Player;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (IsActive)
            {
                RotateBots();
            }
        }

        protected override void ExecuteGroupBehaviour()
        {
            int positionInGroup = 1;
            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemy enemy = Enemies[i];
                if (enemy is EnemyCannonBot bot)
                {
                    Vector3 point = GetNextPoint(positionInGroup, bot);

                    bot.Controller.AddCommand(new MoveToPositionCommand(point));
                    bot.Controller.AddCommand(new RotateTowardsCommand(_player.GlobalPosition));
                }
                positionInGroup++;
            }
        }

        private void RotateBots()
        {
            foreach (Enemy enemy in Enemies)
            {
                if (enemy is EnemyCannonBot bot)
                {
                    bot.Controller.AddCommand(new RotateTowardsCommand(_player.GlobalPosition));
                }
            }
        }

        private Vector3 GetNextPoint(int pointCounter, Enemy enemy)
        {
            Vector3 leadBotPos = _levelCenter;
            if (Enemies.Count > 0)
            {
                leadBotPos = Enemies[0].GlobalPosition;
            }

            Vector3 direction = (leadBotPos - _player.GlobalPosition).Normalized();
            Vector3 p1 = _player.GlobalPosition + (direction * _distanceToPlayer);
            Vector3 ortho = direction.Cross(Vector3.Up);

            //Vector3 point1 = _player.GlobalPosition + new Vector3(0, 0, -_distanceToPlayer);

            switch (pointCounter)
            {
                case 1:
                    return p1;
                case 2:
                    return p1 + (direction * _distanceToPlayer) / 2 + ortho * _distanceToPlayer / 2;
                case 3:
                    return p1 + (direction * _distanceToPlayer) / 2 - ortho * _distanceToPlayer / 2;
                case 4:
                    return p1 + direction * _distanceToPlayer;

                default:
                    GD.PrintErr("Diamond group attempting to assign position over group size.");
                    return Vector3.Zero;
            }
        }
    }
}
