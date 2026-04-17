// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano tuominen.mika-95@hotmail.com

using Godot;

namespace EHE.BoltBusters
{
    public partial class Enemy : Character
    {
        #region EnemyInfo
        private EnemyType _enemyType;
        public EnemyType EnemyType
        {
            get => _enemyType;
            private set => _enemyType = value;
        }

        [Signal]
        public delegate void EnemyDiedEventHandler(int enemyType, Vector3 deathPosition);
        #endregion EnemyInfo

        #region Public methods
        public void Initialize(EnemyType enemyType)
        {
            EnemyType = enemyType;
        }

        public override void OnSpawn() { }

        public override void HandleDeath()
        {
            base.HandleDeath();
            EmitSignal(SignalName.EnemyDied, (int)_enemyType, GlobalPosition);
            OnDespawn();
        }

        // Add additional logic if it differs from default (Node.QueueFree) method.
        public override void OnDespawn()
        {
            base.OnDespawn();
        }
        #endregion Public methods
    }
}
