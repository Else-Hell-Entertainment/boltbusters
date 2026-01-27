using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    public partial class PlayerWeaponGroupController : Node3D, IAttacker
    {
        [Export]
        private string _weaponScenePath = "res://Scenes/Player/Weapons/Chaingun.tscn";

        private List<Node3D> _weaponSlots = new List<Node3D>();
        protected List<BaseWeapon> _weapons = new List<BaseWeapon>();

        public override void _Ready()
        {
            var nodes = GetChildren();
            foreach (var node in nodes)
            {
                string name = node.Name;
                name = name.ToLower();
                if (node is Node3D node3D && name.Contains("slot"))
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
            foreach (BaseWeapon weapon in _weapons)
            {
                if (weapon.CanAttack())
                {
                    weapon.Attack();
                }
            }
        }

        public virtual void AddWeapon()
        {
            if (_weapons.Count >= _weaponSlots.Count)
            {
                GD.Print("Not enough slots");
            }
            else
            {
                var scene = GD.Load<PackedScene>(_weaponScenePath);
                BaseWeapon weapon = scene.Instantiate<BaseWeapon>();
                _weapons.Add(weapon);
                int newIndex = _weapons.Count - 1;
                Node3D node = _weaponSlots[newIndex];
                weapon.Position = node.GetPosition();
                AddChild(weapon);
            }
        }

        public void RemoveWeapon()
        {
            if (_weapons.Count > 0)
            {
                int lastIndex = _weapons.Count - 1;
                _weapons.RemoveAt(lastIndex);
            }
        }
    }
}
