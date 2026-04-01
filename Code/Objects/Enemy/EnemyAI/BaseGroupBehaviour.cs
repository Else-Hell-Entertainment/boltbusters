// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters.EnemyAI
{
    /// <summary>
    /// Base class for enemy group logic and membership management.
    /// </summary>
    public abstract partial class BaseGroupBehaviour : Node3D
    {
        /// <summary>
        /// Current enemies registered to this group.
        /// </summary>
        protected List<Enemy> Enemies { get; private set; } = new List<Enemy>();

        /// <summary>
        /// Enemy type accepted by this group.
        /// </summary>
        protected abstract EnemyType AcceptedEnemyType { get; }

        /// <summary>
        /// Maximum number of enemies in this group.
        /// </summary>
        protected abstract int GroupSize { get; }

        /// <summary>
        /// Enables or disables execution for this group.
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// Tries to register an enemy to this group.
        /// </summary>
        /// <param name="bot">Enemy to register.</param>
        /// <returns>True if registration succeeded.</returns>
        public bool RegisterBot(Enemy bot)
        {
            if (bot.EnemyType != AcceptedEnemyType || Enemies.Count >= GroupSize || Enemies.Contains(bot))
            {
#if DEBUG
                GD.Print("Bot " + bot + " was not accepted to group " + Name);
#endif
                return false;
            }
            Enemies.Add(bot);
#if DEBUG
            GD.Print("Bot " + bot + " was accepted to group " + Name);
#endif
            return true;
        }

        /// <summary>
        /// Removes an enemy from this group.
        /// </summary>
        /// <param name="bot">Enemy to remove.</param>
        /// <returns>True if the enemy was removed.</returns>
        public bool UnRegisterBot(Enemy bot)
        {
            if (Enemies.Contains(bot))
            {
                Enemies.Remove(bot);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes all enemies from this group.
        /// </summary>
        public void ClearBots()
        {
            Enemies.Clear();
        }

        /// <summary>
        /// Validates members and executes the group behaviour when active.
        /// </summary>
        public void Execute()
        {
            if (!IsActive)
            {
                return;
            }
            ValidateGroup();
            ExecuteGroupBehaviour();
        }

        /// <summary>
        /// Removes any references to disposed enemies. This
        /// </summary>
        private void ValidateGroup()
        {
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                if (!IsInstanceValid(Enemies[i]))
                {
                    Enemies.RemoveAt(i);
#if DEBUG
                    GD.Print("Removing invalid enemy entry from " + Name);
#endif
                }
            }
        }

        /// <summary>
        /// Executes behaviour logic for the current group members.
        /// </summary>
        protected abstract void ExecuteGroupBehaviour();
    }
}
