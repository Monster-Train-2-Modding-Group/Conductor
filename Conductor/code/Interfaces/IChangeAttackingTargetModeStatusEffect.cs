using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Interfaces
{
    [Obsolete("Use IStatusEffectChangeAttackingTargetMode instead. This interface will be removed on 11/24/2026.")]
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

    public interface IStatusEffectChangeAttackingTargetMode
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
