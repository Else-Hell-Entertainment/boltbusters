// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters.EnemyAI
{
    /// <summary>
    /// Moves ranged cannonbots into a diamond formation near the player.
    /// </summary>
    public partial class GroupBehaviourCannonbotDiamond : BaseGroupBehaviour
    {
        protected override EnemyType AcceptedEnemyType => EnemyType.Ranged;

        protected override int GroupSize => 4;

        /// <summary>
        /// Preferred spacing from the player for this formation.
        /// </summary>
        private float _distanceToPlayer = 6;

        private Player _player;

        //TODO: Fetch dynamically. Hardcoded for testing purposes.
        private Vector3 _levelCenter = new Vector3(25, 0, 25);

        public override void _Ready()
        {
            base._Ready();
            _player = LevelManager.Active.Player;
        }

        /// <summary>
        /// Assigns formation targets to all valid group members.
        /// </summary>
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
                }
                positionInGroup++;
            }
        }

        /// <summary>
        /// Returns the next formation slot world position.
        /// </summary>
        /// <param name="pointCounter">Slot index in the formation.</param>
        /// <param name="enemy">Enemy requesting the slot.</param>
        /// <returns>Target world position for the slot.</returns>
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
                    this.LogError("Diamond group attempting to assign position over group size.");
                    return Vector3.Zero;
            }
        }
    }
}
