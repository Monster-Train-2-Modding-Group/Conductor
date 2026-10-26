using System.Collections;
using static CardManager;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Card effect that performs a Scry drawing the cards selected and optionally discarding the non selected cards.
    /// </summary>
    public class CardEffectScryTargetPile : CardEffectScryTargetPileBase
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.ParamCardFilter.GetFieldName()] = new PropDescription("Card filter"),
                [CardEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("Base number of cards to scry."),
                [CardEffectFieldNames.ParamStr.GetFieldName()] = new PropDescription("Override Localization Key for instructions."),
                [CardEffectFieldNames.ParamCardUpgradeData.GetFieldName()] = new PropDescription("Optional Card Upgrade"),
                [CardEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("Discard non chosen cards")
            };
        }

        protected override IEnumerator OnCardsChosen(List<CardState> chosenCards, List<CardState> scryedCards, CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            var cardManager = coreGameManagers.GetCardManager();
            var discardScryed = cardEffectState.GetParamBool();
            if (discardScryed)
            {
                foreach (var card in scryedCards)
                {
                    if (chosenCards.Contains(card))
                        continue;

                    DiscardCardParams discardCardParams = new()
                    {
                        discardCard = card,
                        wasPlayed = false,
                        triggeredByCard = true,
                        triggeredCard = cardEffectState.GetParentCardState() ?? cardEffectParams.playedCard,
                    };

                    // Prevent card object duplication since the card is in the draw pile.
                    if (cardManager.IsCardInDrawPile(card))
                        cardManager.GetDrawPile().Remove(card);

                    yield return cardManager.DiscardCard(discardCardParams);
                }
            }
            foreach (var card in chosenCards)
            {
                ApplyCardUpgrade(cardEffectState, card, coreGameManagers);
                cardManager.DrawSpecificCard(card, useEffectDelay: false, HandUI.DrawSource.Deck, playedSourceCard: cardEffectParams.playedCard);
            }
        }
    }
}
