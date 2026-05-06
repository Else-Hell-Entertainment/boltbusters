// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano <tuominen.mika-95@hotmail.com>

namespace EHE.BoltBusters
{
    public abstract partial class ShaderComponent
    {
        #region Locking Fields

        // Locking (used by EffectAwaitPolicy.Locked)
        private bool _effectsLocked = false;
        private int _lockOwnerVersion = 0;

        #endregion Locking Fields

        #region Locking Helpers

        /// <summary>
        /// Attempts to lock this component so that other effect starts cannot override
        /// the currently running locked effect.
        /// </summary>
        private bool TryAcquireLock(int myVersion)
        {
            if (_effectsLocked)
            {
                return false;
            }

            _effectsLocked = true;
            _lockOwnerVersion = myVersion;
            return true;
        }

        /// <summary>
        /// Releases the effect lock if the caller is the current lock owner.
        /// </summary>
        private void ReleaseLockIfOwner(int myVersion)
        {
            if (_effectsLocked && _lockOwnerVersion == myVersion)
            {
                _effectsLocked = false;
                _lockOwnerVersion = 0;
            }
        }

        #endregion Locking Helpers
    }
}
