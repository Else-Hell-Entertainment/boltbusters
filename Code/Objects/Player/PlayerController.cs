using Godot;

namespace EHE.BoltBusters
{
    public partial class PlayerController : EntityController
    {
        [Export] private CharacterBody3D _playerBody;
        [Export] private Node3D _bodyNode;
        [Export] private Node3D _turretNode;

        private CB3DMover _playerBodyMover;
        private NodeMover _turretMover;
        private NodeMover _bodyNodeMover;
        [Export] private InputHandler _inputHandler;

        private bool _hasMoveCommand = false;
        private bool _hasRotateCommand = false;



        public override void _Ready()
        {
            _playerBodyMover = new CB3DMover(_playerBody);
            _bodyNodeMover = new NodeMover(_bodyNode);
            _turretMover = new NodeMover(_turretNode);

            // TODO: Remove from here if different input management system gets implemented.
            //_inputHandler = new InputHandler();
            _inputHandler.SetEntityController(this);
        }


        public override void _PhysicsProcess(double delta)
        {
            ExecuteCommandStack();
            ResetCommandState();
        }

        private void ResetCommandState()
        {
            _hasMoveCommand = false;
            _hasRotateCommand = false;
        }

        protected override bool ValidateCommand(ICommand command)
        {
            switch (command)
            {
                case MoveToDirectionCommand moveToDirectionCommand: // Moving the entire player character.
                {
                    if (_hasMoveCommand) // Player can only have one active move command.
                    {
                        return false;
                    }
                    bool success = moveToDirectionCommand.AssignCommand(_playerBodyMover);
                    if (success)
                    {
                        _hasMoveCommand = true;
                        // Player body needs to rotate to the direction of movement independently of turret.
                        Vector3 point = _playerBody.GlobalPosition + moveToDirectionCommand.Direction * 10;
                        RotateTowardsCommand cmd = new RotateTowardsCommand(point);
                        cmd.AssignCommand(_bodyNodeMover);
                        AddValidatedCommand(cmd);
                    }
                    return success;
                }
                case RotateTowardsCommand rotateTowardsCommand: // Rotating the turret.
                {
                    if (_hasRotateCommand) // Player can only have one active rotation command.
                    {
                        return false;
                    }
                    _hasRotateCommand = true;
                    return rotateTowardsCommand.AssignCommand(_turretMover);
                }
                default: // Command not recognized.
                    return false;
            }
        }
    }
}
