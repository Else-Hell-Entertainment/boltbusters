using System.Collections.Generic;
using EHE.BoltBusters.EnemyAI;
using Godot;

namespace EHE.BoltBusters
{
    public partial class EnemyGroupCannonBotSurroundPlayer : Node3D, IEnemyGroup
    {
        public bool IsActive { get; set; }

        private List<EnemyCannonBot> _botGroup = new List<EnemyCannonBot>();

        public int GroupSize { get; private set; } = 8;

        [Export]
        public float DistanceToPlayer { get; private set; } = 8f;

        private CharacterBody3D _player;

        public override void _Ready()
        {
            base._Ready();
            _player = TargetProvider.Instance.Player;
        }

        public void RegisterBot(EnemyCannonBot bot)
        {
            if (_botGroup.Count < GroupSize && !_botGroup.Contains(bot))
            {
                _botGroup.Add(bot);
            }
            else
            {
                GD.PrintErr("Invalid attempt to register CannonBot to Diamond Group");
            }
        }

        public void ExecuteInternal()
        {
            int pointCounter = 0;
            foreach (EnemyCannonBot bot in _botGroup)
            {
                pointCounter++;
                if (!IsInstanceValid(bot))
                {
                    _botGroup.Remove(bot);
                    GD.Print("bot removed");
                    return;
                }
                BotCommands.MoveTowardsPoint(bot, GetNextPoint(pointCounter));
                BotCommands.TurnTowardsPoint(bot, _player.GlobalPosition);
            }
        }

        private Vector3 GetNextPoint(int pointCounter)
        {
            Vector3 playerPosition = _player.GlobalPosition;
            float quarterPosition = Mathf.Sin(45) * DistanceToPlayer;

            switch (pointCounter)
            {
                case 1:
                    return playerPosition + new Vector3(0, 0, DistanceToPlayer);
                case 2:
                    return playerPosition + new Vector3(0, 0, -DistanceToPlayer);
                case 3:
                    return playerPosition + new Vector3(-DistanceToPlayer, 0, 0);
                case 4:
                    return playerPosition + new Vector3(DistanceToPlayer, 0, 0);
                case 5:
                    return playerPosition + new Vector3(quarterPosition, 0, quarterPosition);
                case 6:
                    return playerPosition + new Vector3(-quarterPosition, 0, -quarterPosition);
                case 7:
                    return playerPosition + new Vector3(quarterPosition, 0, -quarterPosition);
                case 8:
                    return playerPosition + new Vector3(-quarterPosition, 0, quarterPosition);

                default:
                    return Vector3.Zero;
            }
        }
    }
}
