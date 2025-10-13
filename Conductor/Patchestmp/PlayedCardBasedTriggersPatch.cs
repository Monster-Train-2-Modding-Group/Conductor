using Conductor.Extensions;
using Conductor.Triggers;
using HarmonyLib;
using System.Collections;
using static CharacterTriggerData;

namespace Conductor.Patches
{
    // Implementation of Penance and the other half of Accursed.
    [HarmonyPatch(typeof(CardManager), nameof(CardManager.FireUnitTriggersForCardPlayed))]
    public class PlayedCardCharacterTriggerTypePatch
    {
        public static IEnumerator Postfix(IEnumerator enumerator, CardManager __instance, CombatManager ___combatManager, RoomManager ___roomManager, AllGameManagers ___allGameManagers,
                                          ICharacterManager characterManager, CardState playedCard, int playedRoomIndex, List<CharacterState> overrideCharacterList, CharacterState characterThatActivatedAbility)
        {
            while (enumerator.MoveNext())
            {
                var currentType = enumerator.Current.GetType();
                if (currentType.DeclaringType == typeof(CombatManager) && currentType.Name.Contains("RunTriggerQueue"))
                {
                    yield return HandlePlayedCardTriggers(__instance, ___combatManager, ___roomManager, ___allGameManagers, characterManager, playedCard, playedRoomIndex, overrideCharacterList, characterThatActivatedAbility);
                }
                yield return enumerator.Current;
            }
        }

        internal static IEnumerator HandlePlayedCardTriggers(CardManager instance, CombatManager combatManager, RoomManager roomManager, AllGameManagers allGameManagers, ICharacterManager characterManager, CardState playedCard, int playedRoomIndex, List<CharacterState> overrideCharacterList, CharacterState characterThatActivatedAbility)
        {
            var triggerOnCardPlayedParams = new TriggerOnCardPlayedParams
            {
                Card = playedCard,
                RoomIndex = playedRoomIndex,
                Room = roomManager.GetRoom(playedRoomIndex),
                CharacterThatActivatedAbility = characterThatActivatedAbility,
                CoreGameManagers = allGameManagers.GetCoreManagers()
            };

            for (int c = 0; c < characterManager.GetNumCharacters(); c++)
            {
                var charState = characterManager.GetCharacter(c);
                if (charState == null)
                {
                    continue;
                }
                if ((overrideCharacterList == null || !charState.IsDestroyed && charState.IsAlive && overrideCharacterList.Contains(charState)) && playedCard.CharacterInRoomAtTimeOfCardPlay(charState))
                {
                    triggerOnCardPlayedParams.Character = charState;
                    foreach (var trigger_test in CharacterTriggerExtensions.TriggersOnCardPlayed)
                    {
                        if (trigger_test.Value(triggerOnCardPlayedParams, out var queueTriggerParams))
                        {
                            combatManager.QueueCustomTrigger(charState, trigger_test.Key, queueTriggerParams);
                        }
                    }
                }
            }
            yield return combatManager.RunTriggerQueue();
        }
    }
}
