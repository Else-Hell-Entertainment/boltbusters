namespace EHE.BoltBusters
{
    /// <summary>
    /// Command that triggers the Attack() method of a target IAttacker object.
    /// Implements the Command pattern to encapsulate command logic.
    /// </summary>
    public class AttackCommand(string weaponType) : ICommand
    {
        /// <summary>
        /// The IAttacker target that will execute the command.
        /// </summary>
        private IAttacker _attacker;

        /// <summary>
        /// Type of the weapon where this attack command originated from.
        /// </summary>
        public string WeaponType { get; } = weaponType;

        /// <summary>
        /// Executes the attack command. If mover is not assigned correctly, nothing happens.
        /// </summary>
        public void Execute()
        {
            _attacker?.Attack();
        }

        /// <summary>
        /// Assigns an IAttacker target to this command. If no attacker is assigned, the
        /// command does nothing.
        /// </summary>
        /// <param name="target">The target object, must be of type IAttacker.</param>
        /// <returns>True if the target is a valid IAttacker; otherwise, false.</returns>
        public bool AssignReceiver(object target)
        {
            if (target is not IAttacker attacker)
                return false;
            _attacker = attacker;
            return true;
        }
    }
}
