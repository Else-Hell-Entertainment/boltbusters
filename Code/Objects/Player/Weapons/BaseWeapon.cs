// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>

using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Base class for any kind of weapon in the game, mainly used for type checking.
    /// </summary>
    public abstract partial class BaseWeapon : Node3D, IAttacker
    {
        public bool CanAttack { get; protected set; } = true;

        public virtual void Attack() { }
    }
}
