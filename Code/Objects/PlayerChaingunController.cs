using Godot;

namespace EHE.BoltBusters
{
    public partial class PlayerChaingunController : PlayerWeaponGroupController
    {
        public override void _Ready()
        {
            base._Ready();
            AddWeapon();
            AddWeapon();
            AddWeapon();
            AddWeapon();
        }
    }
}
