using Godot;

namespace EHE.BoltBusters
{
    [GlobalClass]
    public partial class EnemyHealthComponent : HealthComponent
    {
        [Export]
        private EnemyShaderComponent _enemyShaderComponent;

        public override bool Decrease(int amount)
        {
            _enemyShaderComponent.PlayEnemyDamageFlash();
            return base.Decrease(amount);
        }
    }
}
