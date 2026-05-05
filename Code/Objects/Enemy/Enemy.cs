// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano tuominen.mika-95@hotmail.com

using EHE.Common.Godot.Logging;
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

        protected float _moveSpeed = 4.0f;
        protected float _normalSpeed = 4.0f; // The speed set on the start and which to use when resetting speed
        protected float _afterAttackSpeed = 2.0f;

        public float MoveSpeed => _moveSpeed;
        public float NormalSpeed => _normalSpeed;
        public float AfterAttackSpeed => _afterAttackSpeed;

        [Signal]
        public delegate void EnemyDiedEventHandler(int enemyType, Vector3 deathPosition);
        #endregion EnemyInfo

        #region Public methods
        public void Initialize(EnemyType enemyType)
        {
            EnemyType = enemyType;
        }

        public virtual void SetMoveSpeed(float newSpeed)
        {
            if (newSpeed >= 0)
            {
                _moveSpeed = newSpeed;
            }
            else
            {
                this.LogWarning($"Attempted to set MoveSpeed to a negative value ({newSpeed}).");
            }
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
