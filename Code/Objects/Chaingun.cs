using Godot;

namespace EHE.BoltBusters
{
    public class Chaingun : IAttacker
    {
        public void Attack()
        {
            GD.Print("ChainGun goes PewPewPew");
            GD.Print("More pew.");
        }
    }
}
