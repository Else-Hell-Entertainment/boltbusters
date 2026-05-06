// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano <tuominen.mika-95@hotmail.com>

using System.Threading.Tasks;

namespace EHE.BoltBusters
{
    public interface IPulseEffect
    {
        void Pulse();
        Task PulseAsync(EffectAwaitPolicy policy = EffectAwaitPolicy.Interruptible);
    }
}
