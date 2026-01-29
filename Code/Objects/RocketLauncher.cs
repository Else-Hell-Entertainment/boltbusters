using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Prototype Rocket Launcher. WIP.
    /// </summary>
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
            GetTree().GetRoot().AddChild(rocket);
            rocket.GlobalPosition = _launchPoint.GlobalPosition;
            rocket.GlobalRotation = _launchPoint.GlobalRotation;

            GD.Print("Rocket launcher goes SKÄBÄDÄBÄDÄÄ");
        }
    }
}
