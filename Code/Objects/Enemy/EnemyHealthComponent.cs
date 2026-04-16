using Godot;

namespace EHE.BoltBusters
{
    [GlobalClass]
    public partial class EnemyHealthComponent : HealthComponent
    {
        [Export]
        private EnemyShaderComponent _enemyShaderComponent;

        [Export]
        private AudioStreamPlayer3D _enemyDamageSoundPlayer;

        public override bool Decrease(int amount)
        {
            _enemyShaderComponent.PlayEnemyDamageFlash();
            _enemyDamageSoundPlayer.Play();
            return base.Decrease(amount);
        }
    }
}
