using Conductor.Extensions;
using Conductor.Triggers;
using HarmonyLib;
using ShinyShoe;
using SickDev.DevConsole.Example;
using System.Collections;
using static CardManager;
using static CharacterTriggerData;

namespace Conductor.Patches
{
    // Junk (character), Junk (card), 1/2 of Accursed trigger implementations.
    [HarmonyPatch(typeof(CardManager), nameof(CardManager.DiscardCard))]
    class DiscardCharacterTriggerTypePatch
    {
        public static IEnumerator Postfix(IEnumerator enumerator, CardManager __instance, CombatManager ___combatManager, RoomManager ___roomManager, AllGameManagers ___allGameManagers, DiscardCardParams discardCardParams)
        {
            while (enumerator.MoveNext())
            {
                var currentType = enumerator.Current.GetType();
                if (currentType.DeclaringType == typeof(RelicManager) && currentType.Name.Contains("ApplyOnDiscardRelicEffects"))
                {
                    yield return HandleDiscardTriggers(__instance, ___combatManager, ___roomManager, ___allGameManagers.GetCoreManagers(), discardCardParams);
                }

                yield return enumerator.Current;
            }
        }

        public static IEnumerator HandleDiscardTriggers(CardManager cardManager, CombatManager combatManager, RoomManager roomManager, ICoreGameManagers coreGameManagers, DiscardCardParams discardCardParams)
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

            var room = roomManager.GetRoom(roomManager.GetSelectedRoom());
            var data = new TriggerOnCardDiscardedParams
            {
                DiscardCardParams = discardCardParams,
                RoomIndex = roomManager.GetSelectedRoom(),
                Room = room,
                CoreGameManagers = coreGameManagers
            };

            List<CharacterState> outCharacters;
            using (GenericPools.GetList(out outCharacters))
            {
                room.AddCharactersToList(outCharacters, Team.Type.Monsters);
                foreach (CharacterState item in outCharacters)
                {
                    foreach (var trigger_func in CharacterTriggerExtensions.TriggersOnCardDiscarded)
                    {
                        var trigger = trigger_func.Key;
                        data.Character = item;
                        if (trigger_func.Value(data, out var queueTriggerParams))
                        {
                            combatManager.QueueCustomTrigger(item, trigger, queueTriggerParams);
                        }
                    }                    
                }
                yield return combatManager.RunTriggerQueue();

                outCharacters.Clear();

                room.AddCharactersToList(outCharacters, Team.Type.Heroes);
                foreach (CharacterState item2 in outCharacters)
                {
                    data.Character = item2;
                    foreach (var trigger_func in CharacterTriggerExtensions.TriggersOnCardDiscarded)
                    {
                        var trigger = trigger_func.Key;
                        if (trigger_func.Value(data, out var queueTriggerParams))
                        {
                            combatManager.QueueCustomTrigger(item2, trigger, queueTriggerParams);
                        }
                    }
                }
                yield return combatManager.RunTriggerQueue();
            }

            // TODO abstract out card trigger.
            bool flag = discardCardParams.triggeredByCard && discardCardParams.discardCard.HasTrait(typeof(CardTraitTreasure));
            if (!discardCardParams.wasPlayed || flag)
            {
                var hand = cardManager.GetHand(shouldCopy: true);
                foreach (var card in hand)
                {
                    yield return combatManager.ApplyCardTriggers(CardTriggers.Junk, card, fireAllMonsterTriggersInRoom: false, roomManager.GetSelectedRoom());
                }
            }
        }
    }
}
