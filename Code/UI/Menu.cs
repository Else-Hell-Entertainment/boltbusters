// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using Godot;

namespace EHE.BoltBusters.Ui
{
    public abstract partial class Menu : Control
    {
        public Menu()
        {
            ProcessMode = ProcessModeEnum.Always;
        }
    }
}
