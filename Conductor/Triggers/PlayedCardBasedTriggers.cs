using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static CardManager;

namespace Conductor.Triggers
{
    // Implementation of Penance and the other half of Accursed.
    [HarmonyPatch(typeof(CardManager), nameof(CardManager.FireUnitTriggersForCardPlayed))]
    public class PlayedCardCharacterTriggerTypePatch
    {
        public static IEnumerator Postfix(IEnumerator enumerator, CardManager __instance, CombatManager ___combatManager, RoomManager ___roomManager,
                                          ICharacterManager characterManager, CardState playedCard, int playedRoomIndex, List<CharacterState> overrideCharacterList, CharacterState characterThatActivatedAbility)
        {
            while (enumerator.MoveNext())
            {
                var currentType = enumerator.Current.GetType();
                if (currentType.DeclaringType == typeof(CombatManager) && currentType.Name.Contains("RunTriggerQueue"))
                {
                    yield return HandlePlayedCardTriggers(__instance, ___combatManager, ___roomManager, characterManager, playedCard, playedRoomIndex, overrideCharacterList, characterThatActivatedAbility);
                }
                yield return enumerator.Current;
            }
        }

        public static IEnumerator HandlePlayedCardTriggers(CardManager instance, CombatManager combatManager, RoomManager roomManager, ICharacterManager characterManager, CardState playedCard, int playedRoomIndex, List<CharacterState> overrideCharacterList, CharacterState characterThatActivatedAbility)
        {
            for (int c = 0; c < characterManager.GetNumCharacters(); c++)
            {
                var charState = characterManager.GetCharacter(c);
                if (playedCard.GetCardType() == CardType.Junk || playedCard.GetCardType() == CardType.Blight)
                {
                    if (charState != null)
                    {
                        if (overrideCharacterList != null)
                        {
                            if (!charState.IsDestroyed && charState.IsAlive && overrideCharacterList.Contains(charState) && playedCard.CharacterInRoomAtTimeOfCardPlay(charState))
                            {
                                combatManager.QueueTrigger(charState, CharacterTriggers.Penance, null, canAttackOrHeal: true, canFireTriggers: true, null, 1);
                                combatManager.QueueTrigger(charState, CharacterTriggers.Accursed, null, canAttackOrHeal: true, canFireTriggers: true, null, 1);
                            }
                        }
                        else if (playedCard.CharacterInRoomAtTimeOfCardPlay(charState))
                        {
                            combatManager.QueueTrigger(charState, CharacterTriggers.Penance, null, canAttackOrHeal: true, canFireTriggers: true, null, 1);
                            combatManager.QueueTrigger(charState, CharacterTriggers.Accursed, null, canAttackOrHeal: true, canFireTriggers: true, null, 1);
                        }
                    }
                }
            }
            yield break;
        }
    }
}
