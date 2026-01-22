using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    public abstract partial class EntityController : Node3D
    {
        private Stack<ICommand> _commands = new Stack<ICommand>();

        public void AddCommand(ICommand command)
        {
            if (ValidateCommand(command))
            {
                _commands.Push(command);
            }
        }

        protected void AddValidatedCommand(ICommand command)
        {
            _commands.Push(command);
        }

        protected virtual bool ValidateCommand(ICommand command)
        {
            return true;
        }

        protected void ExecuteCommandStack()
        {
            while (_commands.Count > 0)
            {
                ICommand command = _commands.Pop();
                command.Execute();
            }
        }

        protected void RunNextCommand()
        {
            if (_commands.TryPop(out ICommand command))
            {
                command.Execute();
            }
        }

        protected void ExecuteCommand(ICommand command)
        {
            command.Execute();
        }

        protected void ClearCommandStack()
        {
            _commands.Clear();
        }


        protected ICommand GetNextCommand()
        {
            return _commands.TryPop(out ICommand cmd) ? cmd : null;
        }
    }
}
