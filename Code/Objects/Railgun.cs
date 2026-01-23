using Godot;

namespace EHE.BoltBusters
{
    public class Railgun : IAttacker
    {
        public void Attack()
        {
            GD.Print("Railgun goes WHUMP");
        }

    }
}
