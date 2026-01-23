using Godot;

namespace EHE.BoltBusters
{
    public partial class Chaingun : Node3D, IAttacker
    {
        [Export] private Timer _cooldownTimer;
        [Export] private float _cooldown = 0.5f;

        public bool _canFire = true;

        // TODO: HACK: HORRIBLE: TESTING ONLY: TEMPORARY!!!!
        [Export] private MeshInstance3D _effect;
        [Export] private Timer _effectTimer;
        [Export] private float _effectTime = 0.05f;

        public override void _Ready()
        {
            _cooldownTimer.WaitTime = _cooldown;
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;

            _effectTimer.WaitTime = _effectTime;
            _effectTimer.Timeout += ResetEffect;
            _effect.Visible = false;
        }

        private void ResetEffect()
        {
            _effect.Visible = false;
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

            _effectTimer.Start();
            _effect.Visible = true;
        }
    }
}
