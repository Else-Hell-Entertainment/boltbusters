// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Pekka Heljakka <Pekka.heljakka@tuni.fi>

/// <summary>
/// Defines the contract for implementing the Command pattern.
/// Commands encapsulate actions that can be executed on target objects.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Executes the command on the assigned target.
    /// </summary>
    public void Execute();

    /// <summary>
    /// Assigns a target object to this command.
    /// </summary>
    /// <param name="target">The object on which the command will operate.</param>
    /// <returns>True if the target was successfully assigned; otherwise, false.</returns>
    public bool AssignReceiver(object target);
}
