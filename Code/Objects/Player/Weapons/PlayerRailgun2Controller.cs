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

        private bool _isAttackPressed;
        private int _physFramesCounter;
        private int _attackFramesCounter;

        public override void _Ready()
        {
            base._Ready();
            InitializeNodes();
            _damageData = new DamageData(150, DamageType.Sniper);
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            if (_isAttackPressed)
            {
                _physFramesCounter++;
                if (_physFramesCounter - _attackFramesCounter > 1)
                {
                    GD.PrintErr("Attack released");
                    _isAttackPressed = false;
                    _physFramesCounter = 0;
                    _attackFramesCounter = 0;
                    _activeRailgun?.Discharge();
                }
            }
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
                if (!SetNextActiveRailgun())
                {
                    return;
                }
            }

            if (!_isAttackPressed && !CustomJustPressedInput())
            {
                return;
            }

            // Player must always wait for active railgun to finish discharging before they can attack again.
            if (_activeRailgun.CurrentState == Railgun2.RailgunState.Discharging)
            {
                return;
            }

            if (_activeRailgun.CurrentState == Railgun2.RailgunState.ReadyToFire)
            {
                _activeRailgun.Attack();
            }
            _isAttackPressed = true;
            _attackFramesCounter++;

            if (_activeRailgun.ChargeReady)
            {
                _activeRailgun.Discharge();
                ShootRailgun();
                _isAttackPressed = false;
            }
        }

        private bool CustomJustPressedInput()
        {
            // Player must always wait for the discharge to finish before they can attempt to shoot again.
            if (_activeRailgun.CurrentState == Railgun2.RailgunState.Discharging)
            {
                return false;
            }

            if (SetNextActiveRailgun())
            {
                _activeRailgun.Attack();
            }
            else
            {
                return false;
            }
            _isAttackPressed = true;
            _attackFramesCounter++;
            return true;
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

        private bool SetNextActiveRailgun()
        {
            foreach (BaseWeapon weapon in Weapons)
            {
                if (weapon is Railgun2 railgun && railgun.CurrentState == Railgun2.RailgunState.ReadyToFire)
                {
                    _activeRailgun = railgun;
                    return true;
                }
            }
            return false;
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
