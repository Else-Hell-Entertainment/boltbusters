namespace EHE.BoltBusters.EnemyAI
{
    public interface IEnemyGroup
    {
        public bool IsActive { get; set; }

        public void Execute()
        {
            if (!IsActive)
            {
                return;
            }
            ExecuteInternal();
        }

        protected abstract void ExecuteInternal();
    }
}
