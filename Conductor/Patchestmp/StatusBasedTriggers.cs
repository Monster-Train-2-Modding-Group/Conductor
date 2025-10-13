using Conductor.Extensions;
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
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            int index = -1;

            for (int i = codes.Count - 1; i >= 0; i--)
            {
                var instruction = codes[i];
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
            var allGameManagersField = AccessTools.Field(typeof(CharacterState), "allGameManagers");

            List<CodeInstruction> newInstructions = [
                new CodeInstruction(OpCodes.Ldarg_0),                                    // CharacterState character
                new CodeInstruction(OpCodes.Ldarg_1),                                    // string statusId
                new CodeInstruction(OpCodes.Ldarg_2),                                    // int numStacks
                new CodeInstruction(OpCodes.Ldloc_S, 2),                                 // StatusEffectStack value2
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, allGameManagersField),                // AllGameManagers
                new CodeInstruction(OpCodes.Call, additionalStatusBasedTriggersMethod),  // ModifyStatusCountByOtherStatus(CharacterState, string, int, StatusEffectStack, AllGameManagers)
            ];

            newInstructions[0].labels.AddRange(codes[index].labels);
            codes[index].labels.Clear();

            codes.InsertRange(index, newInstructions);

            return codes;
        }

        public static void AdditionalStatusBasedTriggers(CharacterState character, string statusId, int numStacks, StatusEffectStack statusEffect, AllGameManagers allGameManagers)
        {
            TriggerOnStatusAddedParams triggerParams = new()
            {
                StatusId = statusId,
                Character = character,
                NumStacks = numStacks,
                StatusEffectStack = statusEffect,
                Room = character.GetCurrentRoom(),
                RoomIndex = character.GetCurrentRoomIndex(),
                CoreGameManagers = allGameManagers.GetCoreManagers()
            };
            
            foreach (var trigger_func in CharacterTriggerExtensions.TriggersOnStatusAdded)
            {
                if (trigger_func.Value(triggerParams, out var queueTriggerParams))
                {
                    allGameManagers.GetCombatManager()!.QueueCustomTrigger(character, trigger_func.Key, queueTriggerParams);
                }
            }
        }
    }

    [HarmonyPatch(typeof(StatusEffectManager), nameof(StatusEffectManager.ShouldTriggerStatusEffectOnUnit))]
    public class StatusEffectManager_ShouldTriggerStatusBasedTriggerOnUnit
    {
        public static void Postfix(ref bool __result, CharacterTriggerData.Trigger triggerType, StatusEffectData.TriggerStage triggerStage)
        {
            if (__result)
                return;

            if (triggerStage != StatusEffectData.TriggerStage.OnPreCharacterTrigger)
                return;

            if (CharacterTriggerExtensions.PreCharacterTriggerAllowedTriggers.Contains(triggerType))
                __result = true;
        }
    }
}
