using System;
using Godot;

namespace EHE.BoltBusters
{
    public partial class EnemyCannonController : EntityController
    {
        [ExportGroup("Node assignment")]
        [Export]
        private CB3DMover _enemyBodyMover;

        [Export]
        private Node3DMover _bodyNodeMover;

        [Export]
        private Node3DMover _turretMover;

        [ExportGroup("Speed settings")]
        [Export]
        private float _movementSpeed = 5f;

        [Export]
        private float _rotationSpeed = 5f;

        [ExportGroup("Navigation settings")]
        [Export]
        private bool _navigationEnabled = true;

        private bool _hasMovementCommand;
        private bool _hasRotationCommand;

        private Vector3 _targetPosition = Vector3.Zero;
        private NavigationAgent3D _navigationAgent;

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
            _enemyBodyMover.MovementSpeed = _movementSpeed;
            _enemyBodyMover.RotationSpeed = _rotationSpeed;
            _turretMover.RotationSpeed = _rotationSpeed;
            _navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
            _enemyBodyMover.EnableNavigation(_navigationAgent);
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
                case MoveToPositionCommand cmd:
                    return cmd.AssignReceiver(_enemyBodyMover);
                default:
                    return false;
            }
        }
    }
}
