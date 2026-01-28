using Godot;

namespace EHE.BoltBusters
{
    public partial class RocketLauncher : BaseWeapon
    {
        [Export]
        private PackedScene _rocketScene;

        private Node3D _launchPoint;

        public override void _Ready()
        {
            _launchPoint = GetNode<Node3D>("LaunchPoint");
        }

        public override void Attack()
        {
            Rocket rocket = _rocketScene.Instantiate<Rocket>();
            rocket.Position = _launchPoint.GlobalPosition;
            rocket.Rotation = _launchPoint.GlobalRotation;
            AddChild(rocket);
            GD.Print("Rocket launcher goes SKÄBÄDÄBÄDÄÄ");
        }
    }
}
