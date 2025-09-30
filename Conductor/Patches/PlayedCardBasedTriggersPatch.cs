using Conductor.Triggers;
using HarmonyLib;
using System.Collections;

namespace Conductor.Patches
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

        // TODO return a Tuple with Trigger, triggerFireCount.
        // Do not patch.
        public static CharacterTriggerData.Trigger[] GetCustomTriggersForCardPlayed(CardState playedCard)
        {
            if (playedCard.GetCardType() == CardType.Junk || playedCard.GetCardType() == CardType.Blight)
                return [CharacterTriggers.Penance, CharacterTriggers.Accursed];
            else if (playedCard.IsUnitAbility())
                return [CharacterTriggers.Evoke];
            return Array.Empty<CharacterTriggerData.Trigger>();
        }

        public static IEnumerator HandlePlayedCardTriggers(CardManager instance, CombatManager combatManager, RoomManager roomManager, ICharacterManager characterManager, CardState playedCard, int playedRoomIndex, List<CharacterState> overrideCharacterList, CharacterState characterThatActivatedAbility)
        {
            var triggersToFire = GetCustomTriggersForCardPlayed(playedCard);
            for (int c = 0; c < characterManager.GetNumCharacters(); c++)
            {
                var charState = characterManager.GetCharacter(c);
                if (charState == null)
                {
                    continue;
                }
                if ((overrideCharacterList == null || !charState.IsDestroyed && charState.IsAlive && overrideCharacterList.Contains(charState)) && playedCard.CharacterInRoomAtTimeOfCardPlay(charState))
                {
                    foreach (var trigger in triggersToFire)
                    {
                        combatManager.QueueTrigger(charState, trigger, null, canAttackOrHeal: true, canFireTriggers: true, null, 1);
                    }
                }
            }
            yield break;
        }
    }
}
