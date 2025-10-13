using Conductor.Interfaces;
using Conductor.StatusEffects;
using HarmonyLib;
using ShinyShoe;
using System.Reflection;
using System.Reflection.Emit;
using static CharacterState;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.AddStatusEffect), [typeof(string), typeof(int), typeof(CharacterState.AddStatusEffectParams), typeof(CharacterState), typeof(bool), typeof(bool), typeof(bool), typeof(bool)])]
    class CharacterState_AddStatusEffect_SelfPropagatingStatusEffectImplementationPatch
    {
        static readonly MethodInfo ModifyStatusCountByOtherStatusMethod = typeof(CharacterState_AddStatusEffect_SelfPropagatingStatusEffectImplementationPatch).GetMethod("ModifyStatusCountByOtherStatus", BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo GetRoomStateModifiedStatusEffectCount = typeof(RoomState).GetMethod("GetRoomStateModifiedStatusEffectCount");

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var index = -1;
            for (int i = 0; i < codes.Count - 1; i++)
            {
                var code = codes[i];
                if (code.Calls(GetRoomStateModifiedStatusEffectCount))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                Plugin.Logger.LogError("Expected to find GetRoomStateModifiedStatusEffectCount, but did not. Patch needs to be redone.");
                return instructions;
            }

            if (!(codes[index + 1].opcode == OpCodes.Add && codes[index + 2].opcode == OpCodes.Starg_S))
            {
                Plugin.Logger.LogError("Expected to find the numStacks + result of GetRoomStateModifiedStatusEffectCount operations, but did not. Patch needs to be redone.");
                return instructions;
            }

            List<CodeInstruction> newInstructions = [
                new CodeInstruction(OpCodes.Ldarg_0),                                    // CharacterState character
                new CodeInstruction(OpCodes.Ldarg_1),                                    // statusId
                new CodeInstruction(OpCodes.Ldarg_2),                                    // numStacks
                new CodeInstruction(OpCodes.Ldarg_S, 5),                                 // allowModification
                new CodeInstruction(OpCodes.Call, ModifyStatusCountByOtherStatusMethod), // ModifyStatusCountByOtherStatus(CharacterState, string, int, bool)
                new CodeInstruction(OpCodes.Starg_S, 2),                                 // numStacks = returnValue
            ];

            codes.InsertRange(index + 3, newInstructions);
            return codes;
        }

        public static int ModifyStatusCountByOtherStatus(CharacterState character, string statusId, int numStacks, bool allowModification)
        {
            if (!allowModification)
                return numStacks;

            int ret = numStacks;
            using (GenericPools.GetList(out List<StatusEffectStack> statusEffectStacks))
            {
                character.GetStatusEffects(ref statusEffectStacks);
                foreach (var statusEffectStack in statusEffectStacks)
                {
                    if (statusEffectStack.State is IOnOtherStatusEffectAdded otherStatusEffectAdded)
                    {
                        ret = otherStatusEffectAdded.OnOtherStatusEffectBeingAdded(statusEffectStack.Count, statusId, ret);
                    }
                }
            }

            return ret;
        }
    }
}
