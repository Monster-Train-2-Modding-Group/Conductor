namespace Conductor.Interfaces
{
    /// <summary>
    /// Interface to make a trait have a special effect when Discarded.
    /// </summary>
    public interface ICardTraitOverrideDiscardEffect
    {
        /// <summary>
        /// Override the DiscardEffect when played.
        /// </summary>
        /// <param name="relicManager">RelicManager instance.</param>
        /// <param name="suppressTraitOnDiscard">Which trait is being suppressed on discard.</param>
        /// <param name="currentDiscardEffect">The proposed discard effect.</param>
        /// <returns>The new discard effect.</returns>
        public HandUI.DiscardEffect GetDiscardEffectWhenPlayed(RelicManager relicManager, Type suppressTraitOnDiscard, HandUI.DiscardEffect currentDiscardEffect);
    }
}
