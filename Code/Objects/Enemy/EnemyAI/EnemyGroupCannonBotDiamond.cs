using System.Collections.Generic;
using EHE.BoltBusters.EnemyAI;
using Godot;

namespace EHE.BoltBusters
{
    public partial class EnemyGroupCannonBotDiamond : Node3D, IEnemyGroup
    {
        public bool IsActive { get; set; }

        private List<EnemyCannonBot> _botGroup = new List<EnemyCannonBot>();

        public int GroupSize { get; private set; } = 4;

        [Export]
        public float DistanceToPlayer { get; private set; } = 7;

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
            int pointCounter = 1;
            foreach (EnemyCannonBot bot in _botGroup)
            {
                if (!IsInstanceValid(bot))
                {
                    _botGroup.Remove(bot);
                    GD.Print("bot removed");
                    return;
                }
                BotCommands.MoveTowardsPoint(bot, GetNextPoint(pointCounter));
                BotCommands.TurnTowardsPoint(bot, _player.GlobalPosition);
                pointCounter++;
            }
        }

        private Vector3 GetNextPoint(int pointCounter)
        {
            Vector3 point1 = _player.GlobalPosition + new Vector3(0, 0, -DistanceToPlayer);

            switch (pointCounter)
            {
                case 1:
                    return point1;
                case 2:
                    return point1 + new Vector3(0, 0, -DistanceToPlayer * 2);
                case 3:
                    return point1 + new Vector3(-DistanceToPlayer, 0, -DistanceToPlayer);
                case 4:
                    return point1 + new Vector3(DistanceToPlayer, 0, -DistanceToPlayer);
                default:
                    return Vector3.Zero;
            }
        }
    }
}
