using Conductor.Extensions;
using Conductor.Interfaces;
using HarmonyLib;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using static TargetHelper;

namespace Conductor.Patches
{
    /// <summary>
    /// Patch to call ITargetSelector.PreCollectTargets and allow patches below to work.
    /// </summary>
    [HarmonyPatch()]
    public class CustomTargetMode_EntryPatch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(TargetHelper), nameof(TargetHelper.CollectTargets), [typeof(CardEffectState), typeof(CardEffectParams), typeof(ICoreGameManagers), typeof(bool)]);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            var methodCallPreTargetFunction = AccessTools.Method(typeof(CustomTargetMode_EntryPatch), "PreTargetFunction");
            // Equivalent to a prefix patch.
            codes.InsertRange(0, [
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Call, methodCallPreTargetFunction),
             ]);

            // Replace this if statement
            // else if (data.targetMode == TargetMode.DrawPile || data.targetMode == TargetMode.Discard || data.targetMode == TargetMode.Exhaust || data.targetMode == TargetMode.LastDrawnCard || data.targetMode == TargetMode.Eaten)
            // with
            // else if (data.targetMode == TargetMode.DrawPile || data.targetMode == TargetMode.Discard || data.targetMode == TargetMode.Exhaust || data.targetMode == TargetMode.LastDrawnCard || IsCardTargetingMode(data.targetMode) || data.targetMode == TargetMode.Eaten)
            int insertLoc = -1;
            object? branchTarget = null;
            for (int i = 0; i < codes.Count(); i++)
            {
                var instruction = codes[i];
                if (branchTarget == null && (instruction.opcode == OpCodes.Beq_S || instruction.opcode == OpCodes.Beq))
                {
                    branchTarget = instruction.operand;
                }

                if (instruction.opcode == OpCodes.Ldc_I4_S && (sbyte)instruction.operand == (sbyte)TargetMode.Eaten)
                {
                    insertLoc = i - 2;
                    break;
                }
            }

            if (branchTarget == null)
            {
                Plugin.Logger.LogError($"------------------------------------------------------------------");
                Plugin.Logger.LogError($"Could not find branch target for patch CustomTargetMode_EntryPatch");
                Plugin.Logger.LogError($"------------------------------------------------------------------");
                return codes;
            }

            var targetModeField = AccessTools.Field(typeof(TargetHelper.CollectTargetsData), "targetMode");
            var methodIsCardTargetingMode = AccessTools.Method(typeof(CustomTargetMode_EntryPatch), "IsCardTargetingMode");

            if (insertLoc > 0) 
            {
                codes.InsertRange(insertLoc, [
                    // Load loc.2
                    new CodeInstruction(OpCodes.Ldloc_2),
                    // Load field targetMode
                    new CodeInstruction(OpCodes.Ldfld, targetModeField),
                    // Call our helper
                    new CodeInstruction(OpCodes.Call, methodIsCardTargetingMode),
                    // If true, branch to IL_0244 like the others
                    new CodeInstruction(OpCodes.Brtrue_S, branchTarget)
                 ]);
            }

            return codes;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "<Pending>")]
        public static bool IsCardTargetingMode(TargetMode targetMode)
        {
            return targetMode.IsCardTargetingMode();
        }

        public static void PreTargetFunction(CardEffectState effectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, bool isTesting)
        {
            if (effectState == null || coreGameManagers.IsNullOrDestroyed())
                return;

            effectState.GetTargetMode().PreCollectTargets(effectState, cardEffectParams, coreGameManagers, isTesting);
        }
    }

    /// <summary>
    /// Patch to call CardTargetSelector.CollectCardTargets
    /// </summary>
    [HarmonyPatch(typeof(TargetHelper), nameof(TargetHelper.CollectCardTargets))]
    public class CustomTargetModes_CardTargetsPatch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "<Pending>")]
        public static void Postfix(TargetHelper.CollectTargetsData data, ICoreGameManagers coreGameManagers, List<CardState> targetCards)
        {
            if (targetCards == null || coreGameManagers.IsNullOrDestroyed())
                return;

            data.targetMode.CollectTargetCards(data, coreGameManagers, targetCards);
        }
    }


    /// <summary>
    /// Patch to call CharacterTargetSelector.CollectTargets (and skips original function if the call returns true)
    /// </summary>
    [HarmonyPatch()]
    public class CustomTargetModePatch2
    {
        private static readonly MethodInfo FilterTowerBossesForMultiRoomAttack = AccessTools.Method(typeof(TargetHelper), "FilterTowerBossesForMultiRoomAttack");

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(TargetHelper), nameof(TargetHelper.CollectTargets), [typeof(CollectTargetsData), typeof(ICoreGameManagers), typeof(List<CharacterState>).MakeByRefType()]);
        }
        
        // TODO merge this with the transpiler patch below. Will only do this if someone wants to postfix patch CollectTargets as it is skipped if CharacterTargetSelector.CollectTargets returns true.
        public static bool Prefix(ref CollectTargetsData data, ICoreGameManagers coreGameManagers, List<CharacterState> targetsOut, List<CharacterState> ___lastTargetedCharacters)
        {
            if (targetsOut == null || coreGameManagers.IsNullOrDestroyed())
                return true;

            var selector = data.targetMode.GetTargetSelector();
            if (selector == null || selector is not CharacterTargetSelector targetSelector)
                return true;

            bool flag = false;
            if (data.firstEffectInPlayedCard != null)
            {
                flag = data.firstEffectInPlayedCard.Value;
            }

            bool flag2 = targetSelector.CollectTargets(data, coreGameManagers, targetsOut);
            if (flag2)
            {
                if (data.isCardEffectDamage)
                {
                    FilterTowerBossesForMultiRoomAttack.Invoke(null, [targetsOut, coreGameManagers.GetRoomManager()]);
                }
                if (flag)
                {
                    ___lastTargetedCharacters.Clear();
                    ___lastTargetedCharacters.AddRange(targetsOut);
                }
                // skip original
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Patch to call CharacterTargetSelector.FilterTargets.
    /// </summary>
    [HarmonyPatch()]
    public class CustomTargetModePatch_FilterTargets
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(TargetHelper), nameof(TargetHelper.CollectTargets), [typeof(CollectTargetsData), typeof(ICoreGameManagers), typeof(List<CharacterState>).MakeByRefType()]);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // Find this section and insert before the "switch" logic
            // end of for loop
            // IL_09eb: ldloc.s 46
            // IL_09ed: ldc.i4.0
            // IL_09ee: bge IL_08c8
            // case TargetMode.Room:
            // IL_09f0: ldloc.s 4
            // IL_09f2: brtrue IL_092a
            int index = -1;
            for (int i = codes.Count - 1; i >= 0; i--)
            {
                if (codes[i].opcode == OpCodes.Ldloc_S && codes[i].LdLocIndex() == 46 &&
                    codes[i + 1].opcode == OpCodes.Ldc_I4_0 &&
                    codes[i + 2].opcode == OpCodes.Bge &&
                    codes[i + 3].opcode == OpCodes.Ldloc_S && codes[i + 3].LdLocIndex() == 4 &&
                    codes[i + 4].opcode == OpCodes.Brtrue)
                {
                    index = i + 3;
                    break;
                }
            }

            if (index == -1)
            {
                Plugin.Logger.LogError($"---------------------------------------------------------------------");
                Plugin.Logger.LogError($"Could not find patch location for CustomTargetModePatch_FilterTargets");
                Plugin.Logger.LogError($"---------------------------------------------------------------------");
                return codes;
            }

            var checkOtherTargetModes = AccessTools.Method(typeof(CustomTargetModePatch_FilterTargets), nameof(CheckOtherTargetModes));
            // (CollectTargetsData data, ICoreGameManagers coreGameManagers, List<CharacterState> allTargets, List<CharacterState> targetsOut)
            List<CodeInstruction> insertedInstructions = [
                new CodeInstruction(OpCodes.Ldarg_0),       // CollectTargetsData data
                new CodeInstruction(OpCodes.Ldarg_1),       // ICoreGameManagers coreGameManagers
                new CodeInstruction(OpCodes.Ldloc_S, 24),   // List<CharacterState> list2 (Contains all valid targets)
                new CodeInstruction(OpCodes.Ldarg_S, 2),    // ref List<CharacterState> targetsOut
                new CodeInstruction(OpCodes.Ldind_Ref),     // Dereference or the game will crash can't forward the ref through to the CharacterTargetSelector interface
                new CodeInstruction(OpCodes.Call, checkOtherTargetModes)
            ];
            codes.InsertRange(index, insertedInstructions);

            return codes;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "<Pending>")]
        public static void CheckOtherTargetModes(CollectTargetsData data, ICoreGameManagers coreGameManagers, List<CharacterState> allTargets, List<CharacterState> targetsOut)
        {
            if (targetsOut == null || coreGameManagers.IsNullOrDestroyed())
                return;

            var selector = data.targetMode.GetTargetSelector();

            if (selector == null || selector is not CharacterTargetSelector targetSelector)
                return;

            targetSelector.FilterTargets(data, coreGameManagers, allTargets, targetsOut);
        }
    }

    [HarmonyPatch(typeof(TargetHelper), nameof(TargetHelper.CollectPreviewTargets))]
    class CustomTargetModesPatch_PreviewTargets
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);

            Label myCaseLabel = il.DefineLabel();
            
            int defaultIndex = -1;
            // 1. Find the default case the last AddCharactersToList call. (room.AddCharactersToList at IL_00bc)
            for (int i = codes.Count - 1; i >= 0; i--)
            {
                if (codes[i].opcode == OpCodes.Ldarg_2 &&
                    codes[i + 1].opcode == OpCodes.Ldarg_3 &&
                    codes[i + 2].opcode == OpCodes.Ldind_Ref &&
                    codes[i + 3].opcode == OpCodes.Ldloc_1 &&
                    codes[i + 4].opcode == OpCodes.Ldc_I4_0 &&
                    codes[i + 5].opcode == OpCodes.Ldc_I4_1 &&
                    codes[i + 6].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i + 6].operand)?.Name == "AddCharactersToList")
                {
                    defaultIndex = i; 
                    break;
                }
            }

            if (defaultIndex < 0)
            {
                Plugin.Logger.LogError("-------------------------------------------------------------------------");
                Plugin.Logger.LogError("Could not find default AddCharactersToList call for CollectPreviewTargets");
                Plugin.Logger.LogError("-------------------------------------------------------------------------");
                return codes;
            }

            var defaultCaseFirstInstruction = codes[defaultIndex];
            int retIndex = codes.FindLastIndex(ci => ci.opcode == OpCodes.Ret);

            // 2. Insert predicate check before default
            codes.InsertRange(defaultIndex,
            [
                // The following inserted IL does the following
                // if (IsCharacterTargetMode(targetMode))
                // {
                new CodeInstruction(OpCodes.Ldloc_0), // targetMode
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CustomTargetModesPatch_PreviewTargets), nameof(IsCharacterTargetMode))),
                new CodeInstruction(OpCodes.Brfalse_S, myCaseLabel),

                //     CollectPreviewTargets(targetMode, card, roomManager, room, previewTargets)
                //     GOTO RET (in case any more code is added after the case statement).
                // }
                new CodeInstruction(OpCodes.Ldloc_0),    // targetMode
                new CodeInstruction(OpCodes.Ldarg_0),    // CardState?
                new CodeInstruction(OpCodes.Ldarg_1),    // RoomManager
                new CodeInstruction(OpCodes.Ldarg_2),    // RoomState
                new CodeInstruction(OpCodes.Ldarg_3), 
                new CodeInstruction(OpCodes.Ldind_Ref),  // List<CharacterState> (must dereference parameter or game crash).
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CustomTargetModesPatch_PreviewTargets), nameof(CollectPreviewTargets))),
                new CodeInstruction(OpCodes.Br_S, codes[retIndex].labels.FirstOrDefault())
            ]);

            // 3. Move the labels to jump to the inserted IL otherwise it will be skipped
            // Should be Label7 (from switch IL command) and Label10 (failure case for case TargetMode.FrontInRoomAndRoomAbove).
            defaultCaseFirstInstruction.MoveLabelsTo(codes[defaultIndex]);
            // Add label for if (IsCharacterTargetMode(targetMode)) to branch to if the if statement is false. This executes the original code for it.
            defaultCaseFirstInstruction.WithLabels(myCaseLabel);

            return codes;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "<Pending>")]
        public static bool IsCharacterTargetMode(TargetMode targetMode)
        {
            return targetMode.IsCharacterTargetingMode();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "<Pending>")]
        public static void CollectPreviewTargets(TargetMode targetMode, CardState? card, RoomManager rm, RoomState room, List<CharacterState> targetsOut)
        {
            if (targetsOut == null)
                return;

            var selector = targetMode.GetTargetSelector();
            if (selector == null || selector is not CharacterTargetSelector targetSelector)
                return;

            targetSelector.CollectPreviewTargets(card, rm, room, targetsOut);
        }
    }

    [HarmonyPatch(typeof(TargetModeExtensions), nameof(TargetModeExtensions.TargetModeIsACardPile))]
    class TargetMode_IsACardPile_Patch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "<Pending>")]
        public static void Postfix(TargetMode targetMode, ref bool __result)
        {
            if (__result) return;
            var targetSelector = targetMode.GetTargetSelector();
            if (targetSelector is not CardTargetSelector cardTargetSelector)
                return;

            __result = cardTargetSelector.TargetsCardPile;
        }
    }

    [HarmonyPatch(typeof(TargetModeExtensions), nameof(TargetModeExtensions.GetIsMultiRoomTargeting))]
    class TargetMode_GetIsMultiRoomTargeting_Patch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "<Pending>")]
        public static void Postfix(TargetMode targetMode, ref bool __result)
        {
            if (__result) return;
            var targetSelector = targetMode.GetTargetSelector();
            if (targetSelector is not CharacterTargetSelector characterTargetSelector)
                return;

            __result = characterTargetSelector.TargetsMultipleRooms;
        }
    }

    [HarmonyPatch(typeof(TargetModeExtensions), nameof(TargetModeExtensions.GetIsRoomTargeting))]
    class TargetMode_GetIsRoomTargeting_Patch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "<Pending>")]
        public static void Postfix(TargetMode targetMode, ref bool __result)
        {
            if (__result) return;
            var targetSelector = targetMode.GetTargetSelector();
            if (targetSelector is not CharacterTargetSelector characterTargetSelector)
                return;

            __result = characterTargetSelector.TargetsRoom;
        }
    }

    [HarmonyPatch(typeof(TargetModeExtensions), nameof(TargetModeExtensions.GetCardTargetMode))]
    class TargetMode_GetCardTargetMode_Patch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "<Pending>")]
        public static void Postfix(TargetMode targetMode, ref CardTargetMode __result)
        {
            var targetSelector = targetMode.GetTargetSelector();
            if (targetSelector == null) return;
            __result = targetSelector.CardTargetMode;
        }
    }
}
