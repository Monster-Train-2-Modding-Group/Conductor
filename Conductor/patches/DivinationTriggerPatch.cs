using Conductor.Triggers;
using HarmonyLib;
using ShinyShoe;
using System.Collections;

namespace Conductor.Patches
{

    [HarmonyPatch(typeof(CardManager), nameof(CardManager.ShuffleDeck))]
    public class Divination_QueueTriggerPatch
    {
        public static bool NeedsToFireQueue = false;
        
        public static void Postfix(bool initialShuffle, MonsterManager ___monsterManager, HeroManager ___heroManager, CombatManager ___combatManager)
        {
            if (initialShuffle)
                return;

            using (GenericPools.GetList<CharacterState>(out var currentCharacters))
            {
                ___heroManager.AddCharactersToList(currentCharacters);
                ___monsterManager.AddCharactersToList(currentCharacters);
                foreach (CharacterState currentCharacter in currentCharacters)
                {
                    ___combatManager.QueueTrigger(currentCharacter, CharacterTriggers.Divination);
                }
                // Because this isn't a coroutine need to run RunTriggerQueue later. Hopefully this was called by DrawHand.
                // Unfortunately this can be triggered due to the DrawCards cheat and if that happens its possible that the TriggerQueue doesn't get fired.
                NeedsToFireQueue = true;
            }
        }
    }

    [HarmonyPatch(typeof(CardManager), nameof(CardManager.DrawHand))]
    public class Divination_DrawCardsTriggerPatch
    {
        public static IEnumerator Postfix(IEnumerator __result, CombatManager ___combatManager)
        {
            while (__result.MoveNext())
            {
                yield return __result.Current;
            }
            if (!Divination_QueueTriggerPatch.NeedsToFireQueue)
            {
                yield break;
            }
            yield return ___combatManager.RunTriggerQueue();
        }
    }

    [HarmonyPatch(typeof(CombatManager), nameof(CombatManager.RunTriggerQueue))]
    public class Divination_RunTriggerQueuePatch
    {
        public static void Prefix()
        {
            Divination_QueueTriggerPatch.NeedsToFireQueue = false;
        }
    }
}
