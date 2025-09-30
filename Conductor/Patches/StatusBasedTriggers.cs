using Conductor.Interfaces;
using Conductor.Triggers;
using HarmonyLib;
using ShinyShoe;
using System.Reflection;
using System.Reflection.Emit;
using static ChallengeData;
using static CharacterState;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.AddStatusEffect), [typeof(string), typeof(int), typeof(CharacterState.AddStatusEffectParams), typeof(CharacterState), typeof(bool), typeof(bool), typeof(bool), typeof(bool)])]
    class CharacterState_AddStatusEffect_StatusBasedTriggers
    {
        static readonly MethodInfo ModifyStatusCountByOtherStatusMethod = typeof(CharacterState_AddStatusEffect_SelfPropagatingStatusEffectImplementationPatch).GetMethod("ModifyStatusCountByOtherStatus", BindingFlags.Static | BindingFlags.Public);
        static readonly MethodInfo GetRoomStateModifiedStatusEffectCount = typeof(RoomState).GetMethod("GetRoomStateModifiedStatusEffectCount");

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            int index = -1;

            for (int i = codes.Count - 1; i >= 0; i--)
            {
                var instruction = codes[i];
                Plugin.Logger.LogError($"{i}: {instruction}");
                if (codes[i].opcode == OpCodes.Ldarg_1 &&
                    codes[i + 1].opcode == OpCodes.Ldstr && (string) codes[i + 1].operand == "silenced" &&
                    codes[i + 2].opcode == OpCodes.Call && codes[i + 2].operand is MethodInfo m && m.Name == "op_Equality" &&
                    codes[i + 3].opcode == OpCodes.Brtrue &&
                    codes[i + 4].opcode == OpCodes.Ldarg_1 &&
                    codes[i + 5].opcode == OpCodes.Ldstr && (string)codes[i + 5].operand == "muted" &&
                    codes[i + 6].opcode == OpCodes.Call && codes[i + 6].operand is MethodInfo n && n.Name == "op_Equality" &&
                    codes[i + 7].opcode == OpCodes.Brfalse)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                Plugin.Logger.LogError($"StatusBasedTriggersPatch could not find if (statusId == \"silenced\" || statusId == \"muted\") check. Patch needs to be redone");
                return codes;
            }

            var additionalStatusBasedTriggersMethod = AccessTools.Method(typeof(CharacterState_AddStatusEffect_StatusBasedTriggers), "AdditionalStatusBasedTriggers");
            var combatManagerField = AccessTools.Field(typeof(CharacterState), "combatManager");

            List<CodeInstruction> newInstructions = [
                new CodeInstruction(OpCodes.Ldarg_0),                                    // CharacterState character
                new CodeInstruction(OpCodes.Ldarg_1),                                    // string statusId
                new CodeInstruction(OpCodes.Ldarg_2),                                    // int numStacks
                new CodeInstruction(OpCodes.Ldloc_S, 2),                                 // StatusEffectStack value2
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, combatManagerField),                  // CombatManager
                new CodeInstruction(OpCodes.Call, additionalStatusBasedTriggersMethod),  // ModifyStatusCountByOtherStatus(CharacterState, string, int, StatusEffectStack, CombatManager)
            ];

            newInstructions[0].labels.AddRange(codes[index].labels);
            codes[index].labels.Clear();

            codes.InsertRange(index, newInstructions);

            return codes;
        }

        public static void AdditionalStatusBasedTriggers(CharacterState character, string statusId, int numStacks, StatusEffectStack statusEffect, CombatManager combatManager)
        {
            if (statusEffect.State.GetDisplayCategory() == StatusEffectData.DisplayCategory.Positive)
            {
                int num = character.GetNumberUniqueStatusEffectsInCategory(StatusEffectData.DisplayCategory.Positive, true);
                combatManager.QueueTrigger(character, CharacterTriggers.OnBuffed, fireTriggersData: new FireTriggersData { paramString = statusId, paramInt = num, paramInt2 = statusEffect.Count });
            }
            if (statusEffect.State.GetDisplayCategory() == StatusEffectData.DisplayCategory.Negative)
            {
                int num = character.GetNumberUniqueStatusEffectsInCategory(StatusEffectData.DisplayCategory.Negative, true);
                combatManager.QueueTrigger(character, CharacterTriggers.OnDebuffed, fireTriggersData: new FireTriggersData { paramString = statusId, paramInt = num, paramInt2 = statusEffect.Count });
            }
            // TODO add hook for other folks to register a Trigger as a status based trigger.
        }
    }
}
