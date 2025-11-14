using Conductor.Triggers;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(HeroManager), nameof(HeroManager.ShiftCharacterTrigger))]
    public class OnShiftStatusEffectTriggerStage
    {
        public static IEnumerator Postfix(IEnumerator __result, CharacterState charState, StatusEffectManager ___statusEffectManager)
        {
            while (__result.MoveNext())
                yield return __result.Current;

            StatusEffectState.InputTriggerParams inputTriggerParams = new()
            {
                associatedCharacter = charState
            };
            StatusEffectState.OutputTriggerParams outputTriggerParams = new();
            yield return ___statusEffectManager.TriggerStatusEffectOnUnit(StatusEffectTriggerStages.OnShift, charState, inputTriggerParams, outputTriggerParams);
        }
    }

    [HarmonyPatch(typeof(HeroManager), "PostAscensionCharacterTriggers")]
    public class OnShiftStatusEffectTriggerStagePostAscension
    {
        public static IEnumerator Postfix(IEnumerator __result, List<CharacterState> characters, StatusEffectManager ___statusEffectManager)
        {
            while (__result.MoveNext())
                yield return __result.Current;

            foreach (CharacterState character in characters)
            { 
                StatusEffectState.InputTriggerParams inputTriggerParams = new()
                {
                    associatedCharacter = character
                };
                StatusEffectState.OutputTriggerParams outputTriggerParams = new();
                yield return ___statusEffectManager.TriggerStatusEffectOnUnit(StatusEffectTriggerStages.OnShift, character, inputTriggerParams, outputTriggerParams);
            }
        }
    }
}
