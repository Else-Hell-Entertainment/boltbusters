using Godot;

namespace EHE.BoltBusters
{
    public class ChainGun : IAttacker
    {
        public void Attack()
        {
            GD.Print("ChainGun goes PewPewPew");
        }
    }
}
