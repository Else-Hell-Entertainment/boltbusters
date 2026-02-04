namespace EHE.BoltBusters
{
    public partial class PlayerRailgunController : PlayerWeaponGroupController
    {
        public override void _Ready()
        {
            base._Ready();
            AddWeapon();
            AddWeapon();
            AddWeapon();
            AddWeapon();
        }

        public override void Attack()
        {
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon.CanAttack())
                {
                    weapon.Attack();
                    return;
                }
            }
        }
    }
}
