using Godot;

namespace EHE.BoltBusters
{
    public partial class Chaingun : BaseWeapon
    {
        [Export]
        private Timer _cooldownTimer;

        [Export]
        private float _cooldown = 0.5f;

        [Export]
        private float _accuracy = 0.05f;

        [Export]
        private float _range = 10f;

        private bool _canFire = true;

        private GpuParticles3D _hitParticles;
        private Node3D _muzzle;
        private MeshInstance3D _reticle;

        // TODO: HACK: HORRIBLE: TESTING ONLY: TEMPORARY!!!!
        [Export]
        private MeshInstance3D _effect;

        [Export]
        private Timer _effectTimer;

        [Export]
        private float _effectTime = 0.05f;

        public override void _Ready()
        {
            _muzzle = GetNode<Node3D>("Muzzle");
            _hitParticles = GetNode<GpuParticles3D>("HitParticles");
            _reticle = GetNode<MeshInstance3D>("Reticle");
            Vector3 retPos = _muzzle.Position + new Vector3(0, 0, -_range);
            _reticle.Position = retPos;

            _cooldownTimer.WaitTime = _cooldown;
            _cooldownTimer.Timeout += OnCooldownTimerTimeout;
            _cooldownTimer.OneShot = true;

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

        public override bool CanAttack()
        {
            return _canFire;
        }

        public override void Attack()
        {
            GD.Print("ChainGun goes PewPewPew");
            GD.Print("More pew.");
            _canFire = false;
            _cooldownTimer.Start();

            _effectTimer.Start();
            _effect.Visible = true;
        }

        private void Shoot() { }
    }
}
