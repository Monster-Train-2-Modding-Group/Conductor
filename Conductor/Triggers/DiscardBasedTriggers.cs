using HarmonyLib;
using System.Collections;
using static CardManager;

namespace Conductor.Triggers
{
    // Junk (character), Junk (card), Penance, Accursed trigger implementations.
    [HarmonyPatch(typeof(CardManager), nameof(CardManager.DiscardCard))]
    class DiscardCharacterTriggerTypePatch
    {
        public static IEnumerator Postfix(IEnumerator enumerator, CardManager __instance, CombatManager ___combatManager, RoomManager ___roomManager, DiscardCardParams discardCardParams, bool fromNaturalPlay)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;

            // End of turn discard all cards from hand.
            if (discardCardParams.handDiscarded)
            {
                yield break;
            }

            // If discard was prevented then no.
            foreach (CardTraitState traitState in discardCardParams.discardCard.GetTraitStates())
            {
                if (!traitState.GetIsDiscardable())
                {
                    yield break;
                }
            }

            bool flag = discardCardParams.triggeredByCard && discardCardParams.discardCard.HasTrait(typeof(CardTraitTreasure));

            if ((discardCardParams.wasPlayed || flag) && (discardCardParams.discardCard.GetCardType() == CardType.Junk || discardCardParams.discardCard.GetCardType() == CardType.Blight))
            {
                yield return ___combatManager.ApplyCharacterEffectsForRoom(CharacterTriggers.Penance, ___roomManager.GetSelectedRoom());
            }

            if ((discardCardParams.wasPlayed || discardCardParams.triggeredByCard) && (discardCardParams.discardCard.GetCardType() == CardType.Junk || discardCardParams.discardCard.GetCardType() == CardType.Blight))
            {
                yield return ___combatManager.ApplyCharacterEffectsForRoom(CharacterTriggers.Accursed, ___roomManager.GetSelectedRoom());
            }

            if (!discardCardParams.wasPlayed || flag)
            {
                yield return ___combatManager.ApplyCharacterEffectsForRoom(CharacterTriggers.Junk, ___roomManager.GetSelectedRoom());

                foreach (var card in __instance.GetHand(shouldCopy: true))
                {
                    yield return ___combatManager.ApplyCardTriggers(CardTriggers.Junk, card, fireAllMonsterTriggersInRoom: false, ___roomManager.GetSelectedRoom());
                }
            }
        }
    }
}
