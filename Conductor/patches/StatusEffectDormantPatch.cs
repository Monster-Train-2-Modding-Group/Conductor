using HarmonyLib;
using static StatusEffectState;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(StatusEffectDormantState), nameof(StatusEffectDormantState.TestTrigger))]
    class StatusEffectDormantPatch
    {
        public static bool Prefix(ref bool __result, InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, StatusEffectDormantState __instance)
        {
            if (__instance.GetAssociatedCharacter().HasStatusEffect("silenced") || __instance.GetAssociatedCharacter().HasStatusEffect("muted"))
            {
                __result = false;
            }
            if (inputTriggerParams.canFireTriggers && !__instance.GetAssociatedCharacter().HasStatusEffect("spark"))
            {
                outputTriggerParams.canFireTriggers = false;
                __result = true;
            }
            __result = false;
            // skip original
            return false;
        }
    }
}
