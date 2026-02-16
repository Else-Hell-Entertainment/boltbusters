namespace EHE.BoltBusters
{
    public enum DamageType
    {
        None = 0,
        Chaingun,
        Sniper,
        Missile,
        Melee,
    }

    public class DamageData
    {
        public int Amount { get; private set; }
        public DamageType Type { get; private set; }

        public DamageData()
        {
            Amount = 0;
            Type = DamageType.None;
        }

        public DamageData(int amount, DamageType damageType)
        {
            Amount = amount;
            Type = damageType;
        }
    }
}
