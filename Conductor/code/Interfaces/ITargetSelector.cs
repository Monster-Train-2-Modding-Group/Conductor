using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Interfaces
{
    /// <summary>
    /// Common Interface for Target Selectors. Users should inherit from either CardTargetSelector or CharacterTargetSelector.
    /// </summary>
    public interface ITargetSelector
    {
        /// <summary>
        /// The associated CardTargetMode used for CardUpgradeMaskData filtering.
        /// </summary>
        public CardTargetMode CardTargetMode { get; }

        /// <summary>
        /// Optional function if you need the CardEffectState / CardEffectParams. You can use this function to implement the TargetMode 
        /// by appending cards to CardEffectParams.targetCards or characters to CardEffectParams.targets
        /// </summary>
        /// <param name="effectState"></param>
        /// <param name="cardEffectParams"></param>
        /// <param name="coreGameManagers"></param>
        /// <param name="isTesting"></param>
        public void PreCollectTargets(CardEffectState effectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, bool isTesting);
    }
}
