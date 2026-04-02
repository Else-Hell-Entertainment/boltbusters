// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters.EnemyAI
{
    /// <summary>
    /// Group behaviour for 4 cannonbots which circle around the player.
    /// </summary>
    public partial class GroupBehaviourCannonbotSurround : BaseGroupBehaviour
    {
        protected override EnemyType AcceptedEnemyType => EnemyType.Ranged;

        protected override int GroupSize => 4;

        // Spacing and rotation variables. Don't change these as they're very fiddly to get right.
        private float _distanceToPlayer = 10f;
        private float _angleSpeed = 0.6f;

        private float _angle;

        private Player _player;

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
                _angle += _angleSpeed * (float)delta;
                _angle = Mathf.Wrap(_angle, 0, Mathf.Tau);
            }
        }

        protected override void ExecuteGroupBehaviour()
        {
            Vector3 playerPosition = _player.GlobalPosition;
            Vector3 north = new Vector3(0, 0, -_distanceToPlayer);

            for (int i = 0; i < Enemies.Count; i++)
            {
                Vector3 pos = playerPosition + north.Rotated(Vector3.Up, _angle + (Mathf.Pi / 2) * i);
                Enemy enemy = Enemies[i];
                if (enemy is EnemyCannonBot bot)
                {
                    bot.Controller.AddCommand(new MoveToPositionCommand(pos));
                }
            }
        }
    }
}
