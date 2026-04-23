// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano tuominen.mika-95@hotmail.com

namespace EHE.BoltBusters
{
    /// <summary>
    /// Determines whether an effect can be overridden by other effects.
    /// <para>
    /// Future extension points: this enum can grow to support policies such as <c>Queued</c>
    /// (start after the current effect finishes), <c>Priority</c> (only override if higher priority),
    /// or <c>PerChannel</c> locking (lock only Flash/Pulse/etc. independently).
    /// </para>
    /// </summary>
    public enum EffectAwaitPolicy
    {
        /// <summary>
        /// Effect may end early if another effect starts.
        /// </summary>
        Interruptible = 0,

        /// <summary>
        /// Effect cannot be overridden by other effects until it finishes.
        /// </summary>
        Locked = 1,
    }
}
