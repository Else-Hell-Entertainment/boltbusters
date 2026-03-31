using EHE.BoltBusters;
using EHE.BoltBusters.EnemyAI;

namespace BoltBusters.Code.Objects.Enemy.EnemyAI
{
    public partial class GroupBehaviourCannonbotDiamond : BaseGroupBehaviour
    {
        public override EnemyType AcceptedEnemyType => EnemyType.Ranged;

        public override int GroupSize => 4;

        protected override void ExecuteGroupBehaviour() { }
    }
}
