using System;
using Godot;
using Godot.Collections;

namespace EHE.BoltBusters
{
    public partial class PlayerRailgun2Controller : PlayerWeaponGroupController
    {
        public override WeaponType WeaponType => WeaponType.Railgun;

        private const int COLLISION_MASK_LAYER = 2;

        private Railgun2 _activeRailgun;
        private Node3D _muzzle;
        private ShapeCast3D _shapeCast3D;
        private DamageData _damageData;

        public override void _Ready()
        {
            base._Ready();
            InitializeNodes();
            _damageData = new DamageData(150, DamageType.Sniper);
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
        }

        private void InitializeNodes()
        {
            _muzzle = GetNode<Node3D>("Muzzle");
            _shapeCast3D = GetNode<ShapeCast3D>("ShapeCast3D");
            _shapeCast3D.CollisionMask = COLLISION_MASK_LAYER;
        }

        public override void Attack()
        {
            if (_activeRailgun == null)
            {
                _activeRailgun = GetNextActiveRailgun();
                if (_activeRailgun == null)
                {
                    return;
                }
            }
            _activeRailgun.Attack();
            if (_activeRailgun.ChargeReady)
            {
                _activeRailgun.Discharge();
                ShootRailgun();
                GetNextActiveRailgun();
            }
        }

        private void ShootRailgun()
        {
            GD.Print("RAILGUN GOES KEKEKEKEKEKKEKEKEKE");
            var collisions = _shapeCast3D.CollisionResult;
            foreach (Dictionary collision in collisions)
            {
                if (collision.ContainsKey("collider"))
                {
                    var collider = collision["collider"];
                    Node target = (Node)collider;
                    if (target is IDamageable damageable)
                    {
                        GD.Print("Railgun hit something!" + target);
                        damageable.TakeDamage(_damageData);
                    }
                }
            }
        }

        private Railgun2 GetNextActiveRailgun()
        {
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon is Railgun2 railgun && railgun.CurrentState == Railgun2.RailgunState.ReadyToFire)
                {
                    return railgun;
                }
            }
            return null;
        }
    }
}
