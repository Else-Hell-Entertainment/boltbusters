using EHE.BoltBusters;
using EHE.BoltBusters.EnemyAI;
using Godot;

namespace EHE.BoltBusters.EnemyAI
{
    public partial class GroupBehaviourCannonbotStandoff : BaseGroupBehaviour
    {
        public override EnemyType AcceptedEnemyType => EnemyType.Ranged;

        // Basically infinite.
        public override int GroupSize => 500;

        private float _distanceToPlayer = 7;

        protected override void ExecuteGroupBehaviour()
        {
            foreach (Enemy enemy in Enemies)
            {
                if (enemy is EnemyCannonBot bot)
                {
                    Vector3 direction = (bot.GlobalPosition - LevelManager.Active.Player.GlobalPosition).Normalized();
                    Vector3 target = LevelManager.Active.Player.GlobalPosition + direction * _distanceToPlayer;
                    bot.Controller.AddCommand(new MoveToPositionCommand(target));
                }
            }
        }
    }
}
