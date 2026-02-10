using System;
using Godot;

namespace EHE.BoltBusters
{
    public partial class EnemyCannonController : EntityController
    {
        [Export]
        private CharacterBody3D _enemyBody;

        [Export]
        private Node3D _bodyNode;

        [Export]
        private Node3D _turretNode;

        [Export]
        private float _movementSpeed = 5f;

        [Export]
        private float _rotationSpeed = 5f;

        private CB3DMover _enemyBodyMover;
        private Node3DMover _bodyNodeMover;
        private Node3DMover _turretMover;

        private bool _hasMovementCommand = false;
        private bool _hasRotationCommand = false;

        public override void _Ready()
        {
            base._Ready();
            Initialize();
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            ExecuteCommandStack();
            ResetCommandState();
        }

        private void ResetCommandState()
        {
            _hasMovementCommand = false;
            _hasRotationCommand = false;
        }

        private void Initialize()
        {
            _enemyBodyMover = new CB3DMover(_enemyBody);
            _enemyBodyMover.MovementSpeed = _movementSpeed;
            _enemyBodyMover.RotationSpeed = _rotationSpeed;
            _bodyNodeMover = new Node3DMover(_bodyNode);
            _turretMover = new Node3DMover(_turretNode);
        }

        protected override bool ValidateCommand(ICommand command)
        {
            switch (command)
            {
                case MoveToDirectionCommand cmd:
                    if (_hasMovementCommand)
                    {
                        return false;
                    }
                    _hasMovementCommand = true;
                    return cmd.AssignReceiver(_enemyBodyMover);
                case RotateTowardsCommand cmd:
                    if (_hasRotationCommand)
                    {
                        return false;
                    }
                    _hasRotationCommand = true;
                    return cmd.AssignReceiver(_turretMover);

                default:
                    return false;
            }
        }
    }
}
