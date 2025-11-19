using Conductor.Extensions;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine.TextCore.Text;
using static CharacterState;
using static CharacterTriggerData;
using static CombatManager;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(CombatManager), nameof(CombatManager.QueueTrigger), [typeof(CharacterState), typeof(CharacterTriggerData.Trigger), typeof(CharacterState), typeof(bool), typeof(bool), typeof(CharacterState.FireTriggersData), typeof(int), typeof(CharacterTriggerState)])]
    public class AliasTriggersPatch
    {
        static MethodInfo enqueueMethod =
            AccessTools.Method(typeof(Queue<TriggerQueueData>), nameof(Queue<TriggerQueueData>.Enqueue));

        static MethodInfo handlerMethod =
            AccessTools.Method(typeof(AliasTriggersPatch), nameof(AliasTriggersPatch.OnQueueTrigger));

        static readonly MethodInfo get_TriggerQueue =
            AccessTools.PropertyGetter(typeof(CombatManager), "TriggerQueue");

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = new List<CodeInstruction>(instructions);

            for (int i = 0; i < list.Count; i++)
            {
                var inst = list[i];
                yield return inst;

                if (inst.opcode == OpCodes.Callvirt &&
                    inst.operand is MethodInfo mi &&
                    mi == enqueueMethod)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // __instance
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // character
                    yield return new CodeInstruction(OpCodes.Ldarg_2); // trigger
                    yield return new CodeInstruction(OpCodes.Ldarg_3); // dyingCharacter
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4); // canAttackOrHeal
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5); // canFireTriggers
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6); // fireTriggersData
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 7); // triggerCount
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 8); // exclusiveTrigger

                    yield return new CodeInstruction(OpCodes.Call, handlerMethod);
                }
            }
        }

        internal static void OnQueueTrigger(
            CombatManager combatManager,
            CharacterState character,
            CharacterTriggerData.Trigger trigger,
            CharacterState dyingCharacter,
            bool canAttackOrHeal,
            bool canFireTriggers,
            CharacterState.FireTriggersData fireTriggersData,
            int triggerCount,
            CharacterTriggerState exclusiveTrigger
        )
        {
            var triggerQueue = (Queue<TriggerQueueData>)get_TriggerQueue.Invoke(combatManager, null);

            var aliases = CharacterTriggerExtensions.TriggerAliases.GetValueOrDefault(trigger, null);
            if (aliases == null)
                return;

            foreach (var aliasedTrigger in aliases)
            {
                triggerQueue.Enqueue(new TriggerQueueData
                {
                    character = character,
                    dyingCharacter = dyingCharacter,
                    trigger = aliasedTrigger,
                    canAttackOrHeal = canAttackOrHeal,
                    canFireTriggers = canFireTriggers,
                    fireTriggersData = fireTriggersData,
                    triggerCount = triggerCount,
                    exclusiveTrigger = exclusiveTrigger
                });
            }
        }
    }
}
