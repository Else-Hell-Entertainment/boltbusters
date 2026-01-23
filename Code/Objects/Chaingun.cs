using Godot;

namespace EHE.BoltBusters
{
    public partial class Chaingun : Node3D, IAttacker
    {
        public void Attack()
        {
            GD.Print("ChainGun goes PewPewPew");
            GD.Print("More pew.");
        }
    }
}
