namespace EHE.BoltBusters
{
    public enum DamageType
    {
        None = 0,
        Chaingun,
        Sniper,
        Missile,
    }

    public class Damage
    {
        public int Amount { get; private set; }
        public DamageType Type { get; private set; }

        public Damage()
        {
            Amount = 0;
            Type = DamageType.None;
        }

        public Damage(int amount, DamageType damageType)
        {
            Amount = amount;
            Type = damageType;
        }
    }
}
