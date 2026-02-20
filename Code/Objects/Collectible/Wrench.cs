// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano tuominen.mika-95@hotmail.com

using Godot;

namespace EHE.BoltBusters
{
    public partial class Wrench : Collectible
    {
        public override void OnSpawn()
        {
            // TODO: Add spawn behavior for this collectible.
            // Examples:
            // - Play a spawn animation (hover, spin, or rise)
            // - Play a spawn sound
            // - Trigger a small appearance VFX
            // - Start idle motion such as bobbing or rotating
        }

        public override void OnCollect(CharacterBody3D collector)
        {
            // TODO: Add pickup behavior for this collectible.
            // Examples:
            // - Apply the collectible's reward to the collector (currency or bonus)
            // - Play pickup sound
            // - Trigger pickup VFX at the collectible's position
            // - Display a UI feedback popup if needed

            OnDespawn();
        }

        // TODO: Add despawn behavior for this collectible.
        // Add: public override void OnDespawn()
        // Examples:
        // - Play a brief disappearance animation
        // - Trigger a small particle effect
        // - Clean up any ongoing animations or timers before removal
    }
}
