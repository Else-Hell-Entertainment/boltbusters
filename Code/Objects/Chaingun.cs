using Godot;

namespace EHE.BoltBusters
{
    public partial class Chaingun : Node3D, IAttacker
    {
        [Export] private Timer _cooldownTimer;
        [Export] private float _cooldown = 0.5f;

        private bool _canFire = true;

        public override void _Ready()
        {
            _cooldownTimer.WaitTime = _cooldown;
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
        }

        private void OnCooldownTimerTimeout()
        {
            GD.Print("Chaingun ready to fire.");
            _canFire = true;
        }

        public void Attack()
        {
            GD.Print("ChainGun goes PewPewPew");
            GD.Print("More pew.");
            _canFire = false;
            _cooldownTimer.Start();
        }
    }
}
