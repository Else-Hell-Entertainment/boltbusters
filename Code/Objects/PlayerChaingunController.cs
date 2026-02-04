using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Controls a group of player controlled chainguns of base type BaseWeapon. This controller is used to manage the
    /// chaingun's SFX.
    /// </summary>
    public partial class PlayerChaingunController : PlayerWeaponGroupController
    {
        private float _attackTimer;
        private float _attackInterval = 0.5f;

        // TODO: Implement chainguns automatically adjusting to target. Currently hardcoded!
        [Export]
        private float _range = 7f;

        private Sprite3D _reticle;

        public override void _Ready()
        {
            base._Ready();
            AddWeapon();
            AddWeapon();
            AddWeapon();
            AddWeapon();
            AddWeapon();
            AddWeapon();
            AddWeapon();
            AddWeapon();

            _reticle = GetNode<Sprite3D>("Reticle");
            _reticle.Position -= new Vector3(0, _reticle.GlobalPosition.Y - 0.2f, _range);
        }

        public override void AddWeapon()
        {
            base.AddWeapon();
            SetAttackInterval();
        }

        public override void RemoveWeapon()
        {
            base.RemoveWeapon();
            SetAttackInterval();
        }

        public override void Attack()
        {
            if (_attackTimer < _attackInterval)
                return;
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon.CanAttack())
                {
                    weapon.Attack();
                    _attackTimer = 0;
                    return;
                }
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            float deltaTime = (float)delta;
            if (_attackTimer < _attackInterval)
            {
                _attackTimer += deltaTime;
            }
        }

        /// <summary>
        /// Sets the attack interval based on number of guns and the individual gun's cooldown to create a continuous
        /// firing effect while making sure the individual ROF is still accounted for. Example: if gun's cooldown is
        /// 0.5 seconds and there are 5 guns, interval will be 0.1 seconds so that every gun still fires when ready,
        /// but they don't all fire at once.
        /// </summary>
        private void SetAttackInterval()
        {
            float numberOfGuns = Weapons.Count;

            if (Weapons.Count > 0 && Weapons[0] is Chaingun chaingun)
            {
                float gunCooldown = chaingun.Cooldown;
                _attackInterval = gunCooldown / numberOfGuns; // Denominator is confirmed to be > 0.
            }
        }
    }
}
