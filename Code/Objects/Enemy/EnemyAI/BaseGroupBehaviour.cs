using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters.EnemyAI
{
    public abstract partial class BaseGroupBehaviour : Node3D
    {
        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();

        public abstract EnemyType AcceptedEnemyType { get; }

        public abstract int GroupSize { get; }

        public bool IsActive;

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

        public bool UnRegisterBot(Enemy bot)
        {
            if (Enemies.Contains(bot))
            {
                Enemies.Remove(bot);
                return true;
            }
            return false;
        }

        public void ClearBots()
        {
            Enemies.Clear();
        }

        public void Execute()
        {
            if (!IsActive)
            {
                return;
            }
            ExecuteGroupBehaviour();
        }

        protected abstract void ExecuteGroupBehaviour();
    }
}
