// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

namespace EHE.BoltBusters.EnemyAI
{
    public interface IEnemyGroup
    {
        public bool IsActive { get; set; }

        public void Execute()
        {
            if (!IsActive)
            {
                return;
            }
            ExecuteInternal();
        }

        protected void ExecuteInternal();
    }
}
