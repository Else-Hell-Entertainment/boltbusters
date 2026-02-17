// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>

namespace EHE.BoltBusters
{
    /// <summary>
    /// Interface for object which can execute any kind of attack.
    /// </summary>
    public interface IAttacker
    {
        // Triggers the attack method.
        public void Attack();
    }
}
