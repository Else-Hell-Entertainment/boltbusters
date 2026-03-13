namespace EHE.BoltBusters
{
    public class StopMovementCommand : ICommand
    {
        EntityMover _mover;

        public void Execute()
        {
            _mover.StopMovement();
        }

        public bool AssignReceiver(object target)
        {
            if (target is not EntityMover mover)
            {
                return false;
            }
            _mover = mover;
            return true;
        }
    }
}
