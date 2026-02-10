using Godot;

namespace EHE.BoltBusters
{
    public partial class CannonBotAi : Node3D
    {
        [Export]
        private EnemyCannon _bot;

        private CharacterBody3D _player;

        public override void _Ready()
        {
            base._Ready();
            _player = TargetProvider.Instance.Player;
        }

        public override void _PhysicsProcess(double delta)
        {
            MoveTowardsPlayer();
        }

        private void MoveTowardsPlayer()
        {
            Vector3 direction = (_player.Position - _bot.Position).Normalized();
            _bot.Controller.AddCommand(new MoveToDirectionCommand(direction));
        }
    }
}
