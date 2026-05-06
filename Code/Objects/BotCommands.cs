// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters
{
    public static class BotCommands
    {
        public static void MoveTowardsPoint(EnemyCannonBot bot, Vector3 point)
        {
            Vector3 direction = (point - bot.GlobalPosition);
            if (direction.Length() < 0.2f)
            {
                return;
            }
            bot.Controller.AddCommand(new MoveToDirectionCommand(direction));
        }

        public static void TurnTowardsPoint(EnemyCannonBot bot, Vector3 point)
        {
            bot.Controller.AddCommand(new RotateTowardsCommand(point));
        }
    }
}
