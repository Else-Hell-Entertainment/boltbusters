// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>

using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Controller for the player entity, managing player movement and turret rotation.
    /// Handles command validation and coordinates multiple mover components for the player body and turret.
    /// </summary>
    public partial class PlayerController : EntityController
    {
        /// <summary>
        /// Smoothing factor for body rotation to prevent jittery movement when rotating towards nearby points.
        /// </summary>
        private const float BODY_ROTATION_SMOOTHING = 10f;

        [Export]
        private float _movementSpeed = 10f;

        [Export]
        private float _acceleration = 40f;

        [Export]
        private float _bodyRotationSpeed = 8f;

        [Export]
        private float _turretRotationSpeed = 8f;

        #region Exported Nodes
        /// <summary>
        /// The CharacterBody3D component used for physics-based player movement.
        /// </summary>
        [Export]
        private CharacterBody3D _playerBody;

        /// <summary>
        /// The visual body node that rotates independently to face the movement direction.
        /// </summary>
        [Export]
        private Node3D _bodyNode;

        /// <summary>
        /// The turret node that rotates independently to face the mouse cursor.
        /// </summary>
        [Export]
        private Node3D _turretNode;

        /// <summary>
        /// Input handler that translates player input into commands.
        /// </summary>
        [Export]
        private InputHandler _inputHandler;

        [Export]
        private PlayerChaingunController _chaingunController;

        [Export]
        private PlayerRailgunController _railgunController;

        [Export]
        private PlayerRocketLauncherController _rocketLauncherController;
        #endregion Exported Nodes

        #region Mover Components
        /// <summary>
        /// Mover component that handles physics-based movement of the player body.
        /// </summary>
        private CB3DMover _playerBodyMover;

        /// <summary>
        /// Mover component that handles rotation of the turret towards the mouse position.
        /// </summary>
        private Node3DMover _turret3DMover;

        /// <summary>
        /// Mover component that handles rotation of the body node towards the movement direction.
        /// </summary>
        private Node3DMover _bodyNode3DMover;
        #endregion Mover Components

        #region Command State Flags
        /// <summary>
        /// Flag indicating whether a move command has been processed this frame.
        /// Prevents multiple movement commands from being executed simultaneously.
        /// </summary>
        private bool _hasMoveCommand;

        /// <summary>
        /// Flag indicating whether a rotate command has been processed this frame.
        /// Prevents multiple rotation commands from being executed simultaneously.
        /// </summary>
        private bool _hasRotateCommand;
        #endregion Command State Flags

        /// <summary>
        /// Maps weapon types to their respective weapon controllers for command delegation.
        /// </summary>
        private Dictionary<WeaponType, IAttacker> _weaponControllers;

        public override void _Ready()
        {
            Initialize();
        }

        public override void _PhysicsProcess(double delta)
        {
            ExecuteCommandStack();
            ResetCommandState();
        }

        private void Initialize()
        {
            // Initialize mover components for each controllable part
            _playerBodyMover = new CB3DMover(_playerBody);
            _playerBodyMover.MovementSpeed = _movementSpeed;
            _playerBodyMover.RotationSpeed = _bodyRotationSpeed;
            _playerBodyMover.Acceleration = _acceleration;

            _bodyNode3DMover = new Node3DMover(_bodyNode);
            _bodyNode3DMover.RotationSpeed = _turretRotationSpeed;

            _turret3DMover = new Node3DMover(_turretNode);
            _turret3DMover.RotationSpeed = _turretRotationSpeed;

            // Initialize weapon controller mapping
            _weaponControllers = new Dictionary<WeaponType, IAttacker>
            {
                { WeaponType.Chaingun, _chaingunController },
                { WeaponType.Railgun, _railgunController },
                { WeaponType.Rocket, _rocketLauncherController },
            };

            // TODO: Remove from here if different input management system gets implemented.
            _inputHandler.SetEntityController(this);
        }

        /// <summary>
        /// Resets the command state flags to allow new commands in the next frame.
        /// </summary>
        private void ResetCommandState()
        {
            _hasMoveCommand = false;
            _hasRotateCommand = false;
        }

        /// <summary>
        /// Validates and assigns commands to the appropriate mover components.
        /// Movement commands control the player body and automatically rotate it towards the movement direction.
        /// Rotation commands control the turret to face the mouse cursor.
        /// </summary>
        /// <param name="command">The command to validate and assign.</param>
        /// <returns>True if the command was successfully validated and assigned; otherwise, false.</returns>
        protected override bool ValidateCommand(ICommand command)
        {
            switch (command)
            {
                case MoveToDirectionCommand moveToDirectionCommand:
                    return HandleMoveCommand(moveToDirectionCommand);

                case RotateTowardsCommand rotateTowardsCommand:
                    return HandleRotateCommand(rotateTowardsCommand);

                case RotateToDirectionCommand rotateToDirectionCommand:
                    return HandleRotateCommand(rotateToDirectionCommand);

                case AttackCommand attackCommand:
                    return HandleAttackCommand(attackCommand);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Handles movement commands by assigning them to the player body and auto-rotating the body node.
        /// </summary>
        private bool HandleMoveCommand(MoveToDirectionCommand command)
        {
            // Only allow one move command per frame
            if (_hasMoveCommand)
            {
                return false;
            }

            // Assign movement to the physics body
            if (!command.AssignReceiver(_playerBodyMover))
            {
                return false;
            }

            _hasMoveCommand = true;

            // Automatically rotate the body node to face the movement direction
            if (command.Direction != Vector3.Zero)
            {
                Vector3 smoothedPoint = _playerBody.GlobalPosition + command.Direction * BODY_ROTATION_SMOOTHING;
                RotateTowardsCommand rotateCommand = new RotateTowardsCommand(smoothedPoint);
                rotateCommand.AssignReceiver(_bodyNode3DMover);
                AddValidatedCommand(rotateCommand);
            }

            return true;
        }

        /// <summary>
        /// Handles rotation commands by assigning them to the turret node.
        /// </summary>
        private bool HandleRotateCommand(ICommand command)
        {
            // Only allow one rotation command per frame
            if (_hasRotateCommand)
            {
                return false;
            }

            // Assign rotation to the turret node
            if (!command.AssignReceiver(_turret3DMover))
            {
                return false;
            }

            _hasRotateCommand = true;
            return true;
        }

        /// <summary>
        /// Handles attack commands by delegating to the appropriate weapon controller.
        /// </summary>
        private bool HandleAttackCommand(AttackCommand command)
        {
            if (!_weaponControllers.TryGetValue(command.WeaponType, out IAttacker controller))
            {
                return false;
            }

            return command.AssignReceiver(controller);
        }
    }
}
