using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    public partial class InputHandler : Node
    {
        private const string MOVELEFT = "MoveLeft";
        private const string MOVERIGHT = "MoveRight";
        private const string MOVEUP = "MoveUp";
        private const string MOVEDOWN = "MoveDown";
        private const string FIRECHAINGUN = "FireChaingun";
        private const string FIRERAILGUN = "FireRailgun";
        private const string FIREROCKET = "FireRocket";

        private List<ICommand> _commands = new List<ICommand>();

        public List<ICommand> GetInputCommands()
        {
            _commands.Clear();
            GetMovementInput();
            return _commands;
        }

        private void GetMovementInput()
        {
            Vector2 inputVector = Input.GetVector(MOVELEFT, MOVERIGHT, MOVEDOWN, MOVEUP);
            if (inputVector == Vector2.Zero)
            {
                return;
            }
            Vector3 moveVector = new Vector3(inputVector.X, 0, -inputVector.Y).Normalized();
            _commands.Add(new MoveToDirectionCommand(moveVector));
        }
    }
}
