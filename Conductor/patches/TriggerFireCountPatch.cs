using Conductor.Interfaces;
using HarmonyLib;
using ShinyShoe;
using System.Reflection;
using System.Reflection.Emit;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.GetTriggerFireCount))]
    class CharacterState_GetTriggerFireCount_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo targetMethod = AccessTools.Method(
                typeof(RelicManager),
                nameof(RelicManager.GetTriggerFireCountModifier)
            );

            MethodInfo hookMethod = AccessTools.Method(
                typeof(CharacterState_GetTriggerFireCount_Patch),
                nameof(GetTriggerFireCountModification)
            );

            return new CodeMatcher(instructions)
                // 1. Find the call to RelicManager::GetTriggerFireCountModifier
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, targetMethod))
                // 2. Advance past the Callvirt and the following Add (IL_00b0: add)
                .Advance(2)
                // 3. Insert our call right before IL_00b1: ret
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0), // this
                    new CodeInstruction(OpCodes.Ldarg_1), // trigger
                    new CodeInstruction(OpCodes.Ldarg_2), // triggerStateContext
                    new CodeInstruction(OpCodes.Call, hookMethod), // pushes modification int
                    new CodeInstruction(OpCodes.Add)      // adds modification to the accumulated total
                )
                .InstructionEnumeration();
        }

        static int GetTriggerFireCountModification(CharacterState character, CharacterTriggerData.Trigger trigger, CharacterTriggerState triggerState)
        {
            int num = 0;
            using (GenericPools.GetList<CharacterState.StatusEffectStack>(out var list))
            {
                character.GetStatusEffects(ref list);
                foreach (var statusEffectStack in list)
                {
                    if (statusEffectStack.State is IStatusEffectModifyTriggerFireCount status)
                    {
                        num += status.GetTriggerFireCount(trigger, triggerState);
                    }
                }
            }
            return num;
        }
    }
}
