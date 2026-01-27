using System;
using Godot;

namespace EHE.BoltBusters
{
    public partial class Player : Character
    {
        public override void _EnterTree() { }

        public override void TakeDamage(DamageData damageData)
        {
            base.TakeDamage(damageData);
            GD.Print("Aaaa I'm taking damage!");
        }
    }
}
