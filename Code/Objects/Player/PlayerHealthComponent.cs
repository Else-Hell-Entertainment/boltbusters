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
