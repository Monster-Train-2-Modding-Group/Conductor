using HarmonyLib;
using System.Collections;
using static CardManager;

namespace Conductor.Triggers
{
    // Junk (character), Junk (card), 1/2 of Accursed trigger implementations.
    [HarmonyPatch(typeof(CardManager), nameof(CardManager.DiscardCard))]
    class DiscardCharacterTriggerTypePatch
    {
        public static IEnumerator Postfix(IEnumerator enumerator, CardManager __instance, CombatManager ___combatManager, RoomManager ___roomManager, DiscardCardParams discardCardParams)
        {
            while (enumerator.MoveNext())
            {
                var currentType = enumerator.Current.GetType();
                if (currentType.DeclaringType == typeof(RelicManager) && currentType.Name.Contains("ApplyOnDiscardRelicEffects"))
                {
                    yield return HandleDiscardTriggers(__instance, ___combatManager, ___roomManager, discardCardParams);
                }

                yield return enumerator.Current;
            }
        }

        public static IEnumerator HandleDiscardTriggers(CardManager cardManager, CombatManager combatManager, RoomManager roomManager, DiscardCardParams discardCardParams)
        {
            // End of turn discard all cards from hand.
            if (discardCardParams.handDiscarded)
            {
                yield break;
            }

            // If discard was prevented then no. Have to iterate through all traits for custom ones that aren't discardable.
            foreach (CardTraitState traitState in discardCardParams.discardCard.GetTraitStates())
            {
                // You can freely discard a card with CardTraitFreeze. CardTraitInfusion can not be discarded freely or via end of turn.
                if (!traitState.GetIsDiscardable() && traitState is not CardTraitFreeze)
                {
                    yield break;
                }
            }

            bool flag = discardCardParams.triggeredByCard && discardCardParams.discardCard.HasTrait(typeof(CardTraitTreasure));
            // TODO this code may need to be revisited.
            // triggeredByCard is surely set if wasPlayed == false. However in the future an artifact could discard a card which that field would not be set.
            if (flag && (discardCardParams.discardCard.GetCardType() == CardType.Junk || discardCardParams.discardCard.GetCardType() == CardType.Blight))
            {
                yield return combatManager.ApplyCharacterEffectsForRoom(CharacterTriggers.Penance, roomManager.GetSelectedRoom());
            }
            if (discardCardParams.triggeredByCard && (discardCardParams.discardCard.GetCardType() == CardType.Junk || discardCardParams.discardCard.GetCardType() == CardType.Blight))
            {
                yield return combatManager.ApplyCharacterEffectsForRoom(CharacterTriggers.Accursed, roomManager.GetSelectedRoom());
            }
            if (!discardCardParams.wasPlayed || flag)
            {
                yield return combatManager.ApplyCharacterEffectsForRoom(CharacterTriggers.Junk, roomManager.GetSelectedRoom());

                foreach (var card in cardManager.GetHand(shouldCopy: true))
                {
                    yield return combatManager.ApplyCardTriggers(CardTriggers.Junk, card, fireAllMonsterTriggersInRoom: false, roomManager.GetSelectedRoom());
                }
            }
        }
    }
}
