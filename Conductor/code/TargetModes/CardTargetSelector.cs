using Conductor.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using static TargetHelper;

namespace Conductor.TargetModes
{
    /// <summary>
    /// Class implementing a custom card target mode.
    /// 
    /// Note that class should only be used for implementing completely new, reusable target modes.
    /// For instance a mod may add a new card pile and put cards in this new card pile.
    /// That would be an acceptable use case to then make a new target mode for that card pile.
    /// 
    /// If a TargetMode is just a subset of a existing target mode (i.e. Only spell cards in deck)
    /// It is best to not implement it as a target mode but to use a CardUpgradeMaskData to filter
    /// to just spell cards.
    /// </summary>
    public abstract class CardTargetSelector : ITargetSelector
    {
        /// <summary>
        /// The associated CardTargetMode used for CardUpgradeMaskData filtering.
        /// </summary>
        public virtual CardTargetMode CardTargetMode { get; } = CardTargetMode.Targetless;

        /// <summary>
        /// Does the target mode target a card pile.
        /// </summary>
        public virtual bool TargetsCardPile { get; } = false;

        /// <see cref=">ITargetSelector"/>
        public virtual void PreCollectTargets(CardEffectState effectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, bool isTesting)
        {

        }
        /// <summary>
        /// Get Target Cards. Required if PreCollectTargets is not implemented.
        /// </summary>
        /// <param name="data">Targeting parameters</param>
        /// <param name="coreGameManagers">CoreGameManagers object</param>
        /// <param name="targetsOut">List to place targeted cards</param>
        public virtual void CollectTargetCards(CollectTargetsData data, ICoreGameManagers coreGameManagers, List<CardState> targetsOut)
        {

        }
    }
}
