using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Base class for all entity controllers in the game.
    /// Implements a command pattern system where commands are queued and executed in a stack-based order.
    /// Derived classes can override validation logic to control which commands are accepted.
    /// </summary>
    public abstract partial class EntityController : Node3D
    {
        /// <summary>
        /// Stack of commands to be executed. Commands are processed in LIFO (Last In, First Out) order.
        /// </summary>
        private Stack<ICommand> _commands = new Stack<ICommand>();

        /// <summary>
        /// Adds a command to the command stack after validation.
        /// The command will only be added if it passes the ValidateCommand check.
        /// </summary>
        /// <param name="command">The command to add to the stack.</param>
        public void AddCommand(ICommand command)
        {
            if (ValidateCommand(command))
            {
                _commands.Push(command);
            }
        }

        /// <summary>
        /// Adds a command directly to the command stack without validation.
        /// Used internally by derived classes to add commands that have already been validated.
        /// </summary>
        /// <param name="command">The pre-validated command to add to the stack.</param>
        protected void AddValidatedCommand(ICommand command)
        {
            _commands.Push(command);
        }

        /// <summary>
        /// Validates whether a command should be accepted by this controller.
        /// The base implementation accepts all commands. Derived classes should override this method
        /// to implement specific validation logic based on command type and current state.
        /// </summary>
        /// <param name="command">The command to validate.</param>
        /// <returns>True if the command is valid and should be added to the stack; otherwise, false.</returns>
        protected virtual bool ValidateCommand(ICommand command)
        {
            return true;
        }

        /// <summary>
        /// Executes all commands in the command stack until it is empty.
        /// Commands are executed in LIFO order (last command added is executed first).
        /// This method is typically called once per frame in the physics process.
        /// </summary>
        protected void ExecuteCommandStack()
        {
            while (_commands.Count > 0)
            {
                ICommand command = _commands.Pop();
                command.Execute();
            }
        }

        /// <summary>
        /// Executes only the next command in the stack, if one exists.
        /// Useful for controllers that need more granular control over command execution timing.
        /// </summary>
        protected void RunNextCommand()
        {
            if (_commands.TryPop(out ICommand command))
            {
                command.Execute();
            }
        }

        /// <summary>
        /// Executes a single command immediately without adding it to the command stack.
        /// Bypasses the command queue system for direct execution.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        protected void ExecuteCommand(ICommand command)
        {
            command.Execute();
        }

        /// <summary>
        /// Removes all commands from the command stack without executing them.
        /// Useful for canceling queued actions or resetting the controller state.
        /// </summary>
        protected void ClearCommandStack()
        {
            _commands.Clear();
        }

        /// <summary>
        /// Retrieves and removes the next command from the stack without executing it.
        /// Returns null if the command stack is empty.
        /// </summary>
        /// <returns>The next command in the stack, or null if the stack is empty.</returns>
        protected ICommand GetNextCommand()
        {
            return _commands.TryPop(out ICommand cmd) ? cmd : null;
        }
    }
}
