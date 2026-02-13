using Conductor.Interfaces;
using HarmonyLib;
using ShinyShoe;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.GetAttackTargetMode))]
    public class CharacterState_GetAttackTargetMode_Patch
    {
        public static void Postfix(CharacterState __instance, ref TargetMode __result)
        {
            if (__result == TargetMode.Room)
            {
                if (GetTargetModeFromStatusEffect(__instance, 1, out var targetMode))
                {
                    __result = targetMode;
                }
            }
            else if (__result == TargetMode.BackInRoom)
            {
                if (GetTargetModeFromStatusEffect(__instance, 2, out var targetMode))
                {
                    __result = targetMode;
                }
            }
            else if (__result == TargetMode.FrontInRoom)
            {
                if (GetTargetModeFromStatusEffect(__instance, Int32.MaxValue, out var targetMode))
                {
                    __result = targetMode;
                }
            }
            else
            {
                Plugin.Logger.LogError($"CharacterState_GetAttackTargetMode_Patch returned an unexpected TargetMode {__result}. Patch needs to be redone");
            }
        }

        public static bool GetTargetModeFromStatusEffect(CharacterState character, int precedence, out TargetMode targetMode)
        {
            int priority = Int32.MaxValue;
            bool found = false;
            targetMode = TargetMode.FrontInRoom;
            using (GenericPools.GetList(out List<CharacterState.StatusEffectStack> statusEffectStacks))
            {
                character.GetStatusEffects(ref statusEffectStacks);
                foreach (var statusEffectStack in statusEffectStacks)
                {
                    if (statusEffectStack.State is IChangeAttackingTargetModeStatusEffect statusEffect)
                    {
                        if (statusEffect.Precedence < precedence && statusEffect.Priority < priority)
                        {
                            targetMode = statusEffect.TargetMode;
                            found = true;
                        }
                    }
                }
            }
            return found;
        }
    }
}
