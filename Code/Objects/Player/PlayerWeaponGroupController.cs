using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Base class for a weapon group controller. Can accept a single type of weapon. IMPORTANT: for weapon slots to
    /// work, add any number of Node3D nodes as children of the WeaponSlots node in the editor. Weapons will be spawned
    /// to these points.
    /// </summary>
    public partial class PlayerWeaponGroupController : Node3D, IAttacker
    {
        private List<Node3D> _weaponSlots = new List<Node3D>();

        [Export]
        private PackedScene _weaponScene;

        protected List<BaseWeapon> Weapons = new List<BaseWeapon>();

        public override void _Ready()
        {
            Node3D weaponSlots = GetNode<Node3D>("WeaponSlots");
            foreach (var node in weaponSlots.GetChildren())
            {
                if (node is Node3D node3D)
                {
                    _weaponSlots.Add(node3D);
                }
            }
        }

        /// <summary>
        /// Base implementation will attack with every weapon that is capable of attacking during the frame.
        /// Override this for more sophisticated attack patterns.
        /// </summary>
        public virtual void Attack()
        {
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon.CanAttack())
                {
                    weapon.Attack();
                }
            }
        }

        /// <summary>
        /// Add a new weapon of type BaseWeapon to the controller. Set the appropriate weapon scene in the editor.
        /// </summary>
        public virtual void AddWeapon()
        {
            if (Weapons.Count >= _weaponSlots.Count)
            {
                GD.Print("Not enough slots!");
            }
            else
            {
                var weapon = _weaponScene.Instantiate<BaseWeapon>();
                Weapons.Add(weapon);
                int newIndex = Weapons.Count - 1;
                Node3D node = _weaponSlots[newIndex];
                weapon.Position = node.GetPosition();
                AddChild(weapon);
            }
        }

        /// <summary>
        /// Removes a weapon from the last index of the controller's list (LIFO) and calls QueueFree on it.
        /// </summary>
        public virtual void RemoveWeapon()
        {
            if (Weapons.Count > 0)
            {
                int lastIndex = Weapons.Count - 1;
                BaseWeapon weapon = Weapons[lastIndex];
                Weapons.RemoveAt(lastIndex);
                weapon.QueueFree();
            }
        }
    }
}
