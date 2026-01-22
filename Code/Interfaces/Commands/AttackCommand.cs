namespace EHE.BoltBusters
{
    public class AttackCommand (string weaponType) : ICommand
    {
        private IAttacker _attacker;
        public string WeaponType { get; } = weaponType;
        public void Execute()
        {
            _attacker?.Attack();

        }

        public bool AssignCommand(object target)
        {
            if (target is not IAttacker attacker) return false;
            _attacker = attacker;
            return true;
        }
    }
}
