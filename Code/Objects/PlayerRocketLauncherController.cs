using Godot;

namespace EHE.BoltBusters
{
    public partial class PlayerRocketLauncherController : PlayerWeaponGroupController
    {
        [Export]
        private float _range = 12f;

        private Sprite3D _reticle;

        public override void _Ready()
        {
            base._Ready();
            _reticle = GetNode<Sprite3D>("Reticle");
            _reticle.Position -= new Vector3(0, _reticle.GlobalPosition.Y - 0.2f, _range);
            AddWeapon();
            AddWeapon();
            AddWeapon();
            AddWeapon();
        }

        public override void Attack()
        {
            base.Attack();
        }
    }
}
