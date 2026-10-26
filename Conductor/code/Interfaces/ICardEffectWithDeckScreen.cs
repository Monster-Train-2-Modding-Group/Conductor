namespace Conductor.Interfaces
{
    /// <summary>
    /// Allows a card effect to adjust parts of DeckScreen when using it.
    /// Remember to call DeckScreen.Params.SetRequestingCardEffect(this) in addition to implementing this interface.
    /// </summary>
    // TODO make a public Abstract Base Class so that new functionality won't break mods.
    internal interface ICardEffectWithDeckScreen : ICardEffect
    {
        /// <summary>
        /// Called to sort the cards present.
        /// </summary>
        /// <param name="cardInfos">Card infos present on the DeckScreen.</param>
        /// <param name="deckScreenMode">Deck Screen Mode</param>
        /// <param name="coreGameManagers">CoreGameManagers</param>
        /// <returns>A sorted list of CardInfo objects </returns>
        public List<DeckScreen.CardInfo> SortDeck(List<DeckScreen.CardInfo> cardInfos, DeckScreen.Mode deckScreenMode, ICoreGameManagers coreGameManagers);

        /// <summary>
        /// Called PostUpdate to update highlighting.
        /// </summary>
        /// <param name="cardInfos"></param>
        /// <param name="selectedIndices"></param>
        /// <param name="deckScreenMode"></param>
        /// <param name="coreGameManagers"></param>
        public void UpdateHighlight(List<DeckScreen.CardInfo> cardInfos, List<int> selectedIndices, DeckScreen.Mode deckScreenMode, ICoreGameManagers coreGameManagers);


        /*public bool ReplayRecordSelection(List<DeckScreen.CardInfo> cardInfos, List<int> selectedIndices, DeckScreen.Mode deckScreenMode, ReplayManager replayManager, ICoreGameManagers coreGameManagers);*/

    }
}
