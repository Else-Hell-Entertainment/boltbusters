// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>
//            Miska Rihu <miska.rihu@tuni.fi>

namespace EHE.BoltBusters.Config
{
    /// <summary>
    /// Contains input map strings for all controls.
    /// </summary>
    public static class ControlConfig
    {
        // Movement axis inputs
        public const string MOVE_LEFT = "MoveLeft";
        public const string MOVE_RIGHT = "MoveRight";
        public const string MOVE_UP = "MoveUp";
        public const string MOVE_DOWN = "MoveDown";

        //Rotation axis inputs
        public const string ROTATE_LEFT = "RotateLeft";
        public const string ROTATE_RIGHT = "RotateRight";
        public const string ROTATE_UP = "RotateUp";
        public const string ROTATE_DOWN = "RotateDown";

        // Attack inputs
        public const string FIRE_CHAINGUN = "FireChaingun";
        public const string FIRE_RAILGUN = "FireRailgun";
        public const string FIRE_ROCKET = "FireRocket";

        // Other
        public const string PAUSE_GAME = "Pause";
    }
}
