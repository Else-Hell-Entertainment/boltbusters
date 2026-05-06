// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano <tuominen.mika-95@hotmail.com>

using Godot;

namespace EHE.BoltBusters
{
    [GlobalClass]
    public partial class PlayerHealthComponent : HealthComponent
    {
        [Export]
        private PlayerShaderComponent _playerShaderComponent;

        public override bool Decrease(int amount)
        {
            _playerShaderComponent.PlayPlayerDamageFlash();
            return base.Decrease(amount);
        }
    }
}
