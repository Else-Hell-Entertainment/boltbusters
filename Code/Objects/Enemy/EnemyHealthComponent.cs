// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano <tuominen-mika95@

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
