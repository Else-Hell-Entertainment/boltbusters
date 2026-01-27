using Godot;

namespace EHE.BoltBusters
{
    public abstract partial class Character : CharacterBody3D, IDamageable
    {
        [Export]
        private HealthComponent _healthComponent = null;

        public virtual void Heal(int amount)
        {
            _healthComponent.Increase(amount);
        }

        public virtual void TakeDamage(DamageData damageData)
        {
            _healthComponent.Decrease(damageData.Amount);
        }
    }
}
