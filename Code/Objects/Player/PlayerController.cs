using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    public partial class PlayerController : CharacterBody3D
    {
        private CB3DMover _bodyMover;
        private InputHandler _inputHandler;

        private MoveToDirectionCommand _moveCommand = null;

        public override void _Ready()
        {
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
                if (command is MoveToDirectionCommand && _moveCommand == null)
                {
                    _moveCommand = command as MoveToDirectionCommand;
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
        }
    }
}
