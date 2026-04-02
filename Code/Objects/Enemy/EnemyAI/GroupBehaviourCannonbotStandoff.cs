// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters.EnemyAI
{
    /// <summary>
    /// Fallback ranged group that keeps enemies at standoff distance.
    /// </summary>
    public partial class GroupBehaviourCannonbotStandoff : BaseGroupBehaviour
    {
        protected override EnemyType AcceptedEnemyType => EnemyType.Ranged;

        // Basically infinite as this is the fallback group if others are full.
        protected override int GroupSize => 500;

        /// <summary>
        /// Preferred distance to maintain from the player.
        /// </summary>
        public float DistanceToPlayer = 12;

        private Player _player;

        public override void _Ready()
        {
            base._Ready();
            _player = LevelManager.Active.Player;
        }

        /// <summary>
        /// Updates movement targets to preserve standoff spacing.
        /// </summary>
        protected override void ExecuteGroupBehaviour()
        {
            if (!IsInstanceValid(_player))
            {
                return;
            }
            foreach (Enemy enemy in Enemies)
            {
                if (enemy is EnemyCannonBot bot)
                {
                    Vector3 direction = (bot.GlobalPosition - _player.GlobalPosition).Normalized();
                    Vector3 target = _player.GlobalPosition + direction * DistanceToPlayer;
                    bot.Controller.AddCommand(new MoveToPositionCommand(target));
                }
            }
        }
    }
}
