using Godot;

namespace EHE.BoltBusters
{
    public abstract partial class BaseWeapon : Node3D, IAttacker
    {
        private float _attackInterval;

        public virtual bool CanAttack()
        {
            return true;
        }

        public virtual void Attack() { }
    }
}
