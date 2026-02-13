using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Interfaces
{
    public interface IChangeAttackingTargetModeStatusEffect
    {
        /// <summary>
        /// Precedence between vanilla effects (0: Overrides Sweep, 1: Overrides Sniper)
        /// </summary>
        public int Precedence { get; }
        /// <summary>
        /// General Priority (tiebreaker for mods) the lower the value the higher the priority (for best coordination among mods pick a random number)
        /// </summary>
        public int Priority { get; }
        /// <summary>
        /// Target Mode to use
        /// </summary>
        public TargetMode TargetMode { get; }
    }
}
