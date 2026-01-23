using Godot;
namespace EHE.BoltBusters
{
    public partial class PlayerChaingunController: Node3D, IAttacker
    {

        [Export] private PackedScene _chaingunScene;

        [Export] private Node3D _gunslot1;
        [Export] private Node3D _gunslot2;
        [Export] private Node3D _gunslot3;

        private Chaingun _chaingun1;
        private Chaingun _chaingun2;
        private Chaingun _chaingun3;

        public override void _Ready()
        {
            _chaingun1 = _chaingunScene.Instantiate<Chaingun>();
            _chaingun1.Position = _gunslot1.Position;
            AddChild(_chaingun1);
            _chaingun2 = _chaingunScene.Instantiate<Chaingun>();
            _chaingun2.Position = _gunslot2.Position;
            AddChild(_chaingun2);
            _chaingun3 = _chaingunScene.Instantiate<Chaingun>();
            _chaingun3.Position = _gunslot3.Position;
            AddChild(_chaingun3);

        }


        public void Attack()
        {
            GD.Print("Controller says FIRE");
            if (_chaingun1._canFire)
            {
                _chaingun1.Attack();
            } else if (_chaingun2._canFire)
            {
                _chaingun2.Attack();
            } else if (_chaingun3._canFire)
            {
                _chaingun3.Attack();
            }

        }


    }
}
