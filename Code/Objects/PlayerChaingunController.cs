using Godot;
namespace EHE.BoltBusters
{
    public partial class PlayerChaingunController: Node3D, IAttacker
    {

        [Export] private Node3D _gunslot1;
        [Export] private Node3D _gunslot2;
        [Export] private Node3D _gunslot3;

        private Chaingun _chaingun1;
        private Chaingun _chaingun2;
        private Chaingun _chaingun3;


        public void Attack()
        {

        }


    }
}
