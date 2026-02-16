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

        /// <summary>
        /// Input handler that translates player input into commands.
        /// </summary>
        [Export]
        private InputHandler _inputHandler;

        /// <summary>
        /// Flag indicating whether a move command has been processed this frame.
        /// Prevents multiple movement commands from being executed simultaneously.
        /// </summary>
        private bool _hasMoveCommand = false;

        /// <summary>
        /// Flag indicating whether a rotate command has been processed this frame.
        /// Prevents multiple rotation commands from being executed simultaneously.
        /// </summary>
        private bool _hasRotateCommand = false;

        [Export]
        private PlayerChaingunController _chaingunController;

        [Export]
        private PlayerRailgunController _railgunController;

        [Export]
        private PlayerRocketLauncherController _rocketLauncherController;

        public override void _Ready()
        {
            // Initialize mover components for each controllable part
            _playerBodyMover = new CB3DMover(_playerBody);
            _bodyNode3DMover = new Node3DMover(_bodyNode);
            _turret3DMover = new Node3DMover(_turretNode);

            // TODO: Remove from here if different input management system gets implemented.
            _inputHandler.SetEntityController(this);
        }

        public override void _PhysicsProcess(double delta)
        {
            ExecuteCommandStack();
            ResetCommandState();
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
                case MoveToDirectionCommand moveToDirectionCommand: // Moving the entire player character.
                {
                    // Only allow one move command per frame
                    if (_hasMoveCommand)
                    {
                        return false;
                    }

                    // Assign movement to the physics body
                    bool success = moveToDirectionCommand.AssignReceiver(_playerBodyMover);
                    if (success)
                    {
                        _hasMoveCommand = true;

                        // Automatically rotate the body node to face the movement direction. Direction multiplied by
                        // factor of 10 to smooth out the motion, sometimes if the point is too close the rotation
                        // is jittery.
                        Vector3 point = _playerBody.GlobalPosition + moveToDirectionCommand.Direction * 10;
                        RotateTowardsCommand cmd = new RotateTowardsCommand(point);
                        cmd.AssignReceiver(_bodyNode3DMover);
                        AddValidatedCommand(cmd);
                    }

                    return success;
                }
                case RotateTowardsCommand rotateTowardsCommand: // Rotating the turret.
                {
                    // Only allow one rotation command per frame
                    if (_hasRotateCommand)
                    {
                        return false;
                    }

                    _hasRotateCommand = true;

                    // Assign rotation to the turret node
                    return rotateTowardsCommand.AssignReceiver(_turret3DMover);
                }
                case AttackCommand attackCommand:
                    switch (attackCommand.WeaponType)
                    {
                        case "Chaingun":
                            return attackCommand.AssignReceiver(_chaingunController);
                        case "Railgun":
                            return attackCommand.AssignReceiver(_railgunController);
                        case "Rocket":
                            return attackCommand.AssignReceiver(_rocketLauncherController);
                    }
                    return false;

                default: // Command not recognized.
                    return false;
            }
        }
    }
}
