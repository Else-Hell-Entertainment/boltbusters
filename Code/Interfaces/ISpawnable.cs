// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano tuominen.mika-95@hotmail.com

namespace EHE.BoltBusters
{
    public interface ISpawnable
    {
        // Trigger events that the object needs to handle AFTER it has spawned.
        // Can be used for Start animations, set variables, etc.
        void OnSpawn();

        // Trigger events when the object is called to despawn.
        // Can be used for 'Death' animations, object rest / queue free, etc.
        void OnDespawn();
    }
}
