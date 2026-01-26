using System;
using Godot;

namespace EHE.BoltBusters
{
    public partial class Player : Character
    {
        public override void _EnterTree() { }

        public override void TakeDamage(Damage damageObj)
        {
            base.TakeDamage(damageObj);
            GD.Print("Aaaa I'm taking damage!");
        }
    }
}
