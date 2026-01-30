using System;
using Godot;

namespace EHE.BoltBusters
{
    public partial class Player : Character
    {
        /// <summary>
        /// (Parent) _EnterTree runs first and is good for registering and subscribing to services.
        /// </summary>
        public override void _EnterTree()
        {
            if (TargetProvider.Instance == null)
            {
                GD.PushWarning($"Player: TargetProvider.Instance is null. Player was not registered.");
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

        public override void TakeDamage(DamageData damageData)
        {
            base.TakeDamage(damageData);
            GD.Print("Aaaa I'm taking damage!");
        }
    }
}
