using Godot;

namespace EHE.BoltBusters
{
    [GlobalClass]
    public partial class PlayerHealthComponent : HealthComponent
    {
        [Export]
        private PlayerShaderComponent _playerShaderComponent;

        [Export]
        private AudioStreamPlayer3D _playerDamageSoundPlayer;

        public override bool Decrease(int amount)
        {
            _playerShaderComponent.PlayPlayerDamageFlash();
            _playerDamageSoundPlayer.Play();
            return base.Decrease(amount);
        }
    }
}
