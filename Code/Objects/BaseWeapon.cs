using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Base class for any kind of weapon in the game, mainly used for type checking.
    /// </summary>
    public abstract partial class BaseWeapon : Node3D, IAttacker
    {
        public virtual bool CanAttack()
        {
            return true;
        }

        public virtual void Attack() { }
    }
}
