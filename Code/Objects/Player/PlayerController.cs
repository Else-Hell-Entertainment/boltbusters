using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    public partial class PlayerController : CharacterBody3D
    {
        [Export]
        private CharacterBody3D _turretBody;
        private CB3DMover _bodyMover;
        private CB3DMover _turretMover;
        private InputHandler _inputHandler;

        private MoveToDirectionCommand _moveCommand = null;
        private RotateTowardsCommand _rotateTowardsCommand = null;

        public override void _Ready()
        {
            _turretBody = GetNode<CharacterBody3D>("TurretBody");
            if (_turretBody == null)
            {
                GD.PrintErr("TurretBody3D not assigned to PlayerController.");
            }

            _turretMover = new CB3DMover(_turretBody);
            _bodyMover = new CB3DMover(this);
            _inputHandler = new InputHandler();
        }

        public override void _Process(double delta)
        {
            HandleInput();
        }

        private void HandleInput()
        {
            List<ICommand> commandList = _inputHandler.GetInputCommands();
            foreach (ICommand command in commandList)
            {
                switch (command)
                {
                    case MoveToDirectionCommand moveToDirectionCommand when _moveCommand == null:
                        _moveCommand = moveToDirectionCommand;
                        break;
                    case RotateTowardsCommand rotateTowardsCommand when _rotateTowardsCommand == null:
                        _rotateTowardsCommand = rotateTowardsCommand;
                        break;
                }
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            ExecuteCommands();
        }

        private void ExecuteCommands()
        {
            if (_moveCommand != null)
            {
                _moveCommand.Execute(_bodyMover);
                _moveCommand = null;
            }

            if (_rotateTowardsCommand != null)
            {
                _rotateTowardsCommand.Execute(_turretMover);
                _rotateTowardsCommand = null;
            }
        }
    }
}
