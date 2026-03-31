using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters.EnemyAI
{
    public abstract partial class BaseGroupBehaviour : Node3D
    {
        private List<Enemy> _enemies = new List<Enemy>();

        public virtual EnemyType AcceptedEnemyType { get; private set; }

        public virtual int GroupSize { get; private set; }

        public BaseGroupBehaviour(int groupSize, EnemyType type)
        {
            GroupSize = groupSize;
            AcceptedEnemyType = type;
        }

        public bool RegisterBot(Enemy bot)
        {
            if (bot.EnemyType != AcceptedEnemyType && _enemies.Contains(bot))
            {
                return false;
            }
            _enemies.Add(bot);
            return true;
        }

        public bool UnRegisterBot(Enemy bot)
        {
            if (_enemies.Contains(bot))
            {
                _enemies.Remove(bot);
                return true;
            }
            return false;
        }

        public void ClearBots()
        {
            _enemies.Clear();
        }
    }
}
