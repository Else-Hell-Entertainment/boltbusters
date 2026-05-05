// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <pekka.heljakka@tuni.fi>

using EHE.Common.Godot.Logging;
using Godot;

namespace EHE.BoltBusters
{
    public partial class EnemyCannonController : EntityController
    {
        [ExportGroup("Node assignment")]
        [Export]
        private Enemy _cannonOwner;

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
            // TODO: Refactor if there's ever time (there isn't).
            // Navigation agent should be in it's own class in order to decouple the logic from controller and mover.
            // This is here because it works for now.
            if (_enemyBodyMover.IsMovingToPosition)
            {
                RotateTowardsCommand cmd = new RotateTowardsCommand(_navigationAgent.TargetPosition);
                if (cmd.AssignReceiver(_bodyNodeMover))
                {
                    AddValidatedCommand(cmd);
                }
            }
            ExecuteCommandStack();
            ResetCommandState();
        }

        public void SetMoveSpeed(float newSpeed)
        {
            if (newSpeed >= 0)
            {
                _movementSpeed = newSpeed;
                _enemyBodyMover.MovementSpeed = _movementSpeed;
            }
            else
            {
                this.LogWarning($"Attempted to set MoveSpeed to a negative value ({newSpeed}).");
            }
        }

        public void StopMovement()
        {
            _enemyBodyMover.StopMovement();
        }

        private void ResetCommandState()
        {
            _hasMovementCommand = false;
            _hasRotationCommand = false;
        }

        private void Initialize()
        {
            _movementSpeed = _cannonOwner.MoveSpeed;
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
                    Vector3 rotateDirection = cmd.Direction;
                    Vector3 rotationTarget = GlobalPosition + rotateDirection;
                    RotateTowardsCommand rotCmd = new RotateTowardsCommand(rotationTarget);
                    rotCmd.AssignReceiver(_enemyBodyMover);
                    AddValidatedCommand(rotCmd);
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
                case StopMovementCommand cmd:
                    return cmd.AssignReceiver(_enemyBodyMover);
                default:
                    return false;
            }
        }
    }
}
