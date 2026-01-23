using Godot;

namespace EHE.BoltBuster
{
    public partial class PlayerAddThis : CharacterBody3D
    {
        /// <summary>
        /// (Parent) _EnterTree runs first and is good for registering and subscribing to services.
        /// </summary>
        public override void _EnterTree()
        {
            if (TargetProvider.Instance == null)
            {
                GD.PushWarning($"{nameof(this)}: TargetProvider.Instance is null. Player was not registered.");
            }

            TargetProvider.Instance.RegisterPlayer(this);
        }

        /// <summary>
        /// Remove player from TargetProvider when exiting tree.
        /// </summary>
        public override void _ExitTree()
        {
            TargetProvider.Instance?.UnregisterPlayer(this);
        }
    }
}
