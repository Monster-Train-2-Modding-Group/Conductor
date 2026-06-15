using Conductor.Extensions;
using Conductor.Triggers;
using HarmonyLib;
using ShinyShoe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static CardManager;

namespace Conductor.patches
{
    [HarmonyPatch(typeof(CardManager), nameof(CardManager.PurgeCard))]
    class PurgedBasedTriggersPatch
    {
        public static IEnumerator Postfix(IEnumerator enumerator, CardManager __instance, CombatManager ___combatManager, RoomManager ___roomManager, AllGameManagers ___allGameManagers, CardState cardState)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
            yield return HandlePurgedTriggers(__instance, ___combatManager, ___roomManager, ___allGameManagers.GetCoreManagers(), cardState);
        }

        public static IEnumerator HandlePurgedTriggers(CardManager cardManager, CombatManager combatManager, RoomManager roomManager, ICoreGameManagers coreGameManagers, CardState cardState)
        {
            var room = roomManager.GetRoom(roomManager.GetSelectedRoom());
            var data = new TriggerOnCardPurgedParams
            {
                Card = cardState,
                RoomIndex = roomManager.GetSelectedRoom(),
                Room = room,
                WasEphemeral = CardTraitEphemeralPurgedPatch.cardPurged != null,
                EphemeralCardWasPlayed = CardTraitEphemeralPurgedPatch.cardWasPlayed ?? false,
                CoreGameManagers = coreGameManagers
            };

            List<CharacterState> outCharacters;
            using (GenericPools.GetList(out outCharacters))
            {
                room.AddCharactersToList(outCharacters, Team.Type.Monsters);
                foreach (CharacterState item in outCharacters)
                {
                    foreach (var trigger_func in CharacterTriggerExtensions.TriggersOnCardPurged)
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
                    foreach (var trigger_func in CharacterTriggerExtensions.TriggersOnCardPurged)
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
        }
    }

    [HarmonyPatch(typeof(CardTraitEphemeral), "PurgeCard")]
    internal class CardTraitEphemeralPurgedPatch
    {
        internal static CardState? cardPurged;
        internal static bool? cardWasPlayed;

        public static IEnumerator Postfix(IEnumerator enumerator, ICoreGameManagers coreGameManagers, CardState cardState, bool cardPlayed)
        {
            cardPurged = cardState;
            cardWasPlayed = cardPlayed;
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
            cardPurged = null;
            cardWasPlayed = null;
        }
    }
}
