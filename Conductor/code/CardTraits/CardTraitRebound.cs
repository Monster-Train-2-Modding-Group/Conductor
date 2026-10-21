using System.Collections;
using UnityEngine;

namespace Conductor.CardTraits
{
    public class CardTraitRebound : CardTraitState
    {
        public override IEnumerator OnCardDiscarded(CardManager.DiscardCardParams discardCardParams, ICoreGameManagers coreGameManagers)
        {
            var card = GetCard();
            if (!discardCardParams.isRecastCopy && discardCardParams.wasPlayed && !card.HasTrait<CardTraitExhaustState>())
            {
                CardManager cardManager = coreGameManagers.GetCardManager();
                CombatManager combatManager = coreGameManagers.GetCombatManager();
                SaveManager saveManager = coreGameManagers.GetSaveManager();
                HandUI handUI = coreGameManagers.GetHandUI();
                Vector3 target = AllGameManagers.Instance?.GetGameScreen()?.GetDrawPileLocation(useScreenSpace: true) ?? Vector3.zero;

                // Don't use MoveToStandbyPile because it doesn't support moving the card to a random location in the deck.
                // Must manually manipulate piles.
                //cardManager.MoveToStandByPile(card, discardCardParams.wasPlayed, wasExhausted: false, condition, discardCardParams);
                var deckPile = cardManager.GetDrawPile();
                // Safe. This function is only called while not in a preview.
                int index = RandomManager.Range(0, deckPile.Count, RngId.Battle);
                deckPile?.Insert(index, card);
                cardManager.GetDiscardPile().Remove(card);
                cardManager.GetDiscardBufferPile().Remove(card);

                // Moe throws out barney meme.
                // Unfortunately this code showcases the card you just played which is unwanted.
                /*yield return CoreUtil.WaitForSecondsOrBreak(saveManager.GetAllGameData().GetBalanceData().GetAnimationTimingData().cardDiscardAnimationDuration);
                CardAnimator.CardAnimationParameters type = CardAnimator.CardAnimationParameters.FromCardState(card, handUI, target, Vector3.zero);
                type.discardEffect = HandUI.DiscardEffect.None;
                type.sourcePile = CardPile.HandPile;
                type.maxCardsToShowcase = 0;
                type.sourcePile = CardPile.DiscardPile;
                //var type = CardAnimator.CardAnimationParameters.FromExistingCardUI(cardManager.GetCardInHand(card), handUI, true, HandUI.DiscardEffect.None, 0f, target);
                //type.sourcePile = CardPile.HandPile;
                CardAnimator.DoCardMovementAnimation.Dispatch(type);*/
            }
            yield break;
        }

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return [];
        }

        public override string GetCardText()
        {
            return LocalizeTraitKey("CardTraitRebound_CardText");
        }

        public override string GetCardTooltipTitle()
        {
            return LocalizeTraitKey("CardTraitRebound_TooltipTitle");
        }

        public override string GetCardTooltipText()
        {
            return "CardTraitRebound_TooltipText".Localize();
        }
    }
}
