using Conductor.Extensions;
using HarmonyLib;
using static CardStatistics;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(CardStatistics), nameof(CardStatistics.ResetAllStats))]
    public class CardStatistics_ResetAllStats_Patch
    {
        public static void Postfix()
        {
            foreach (var handler in TrackedValueTypeExtensions.TrackedValueHandlers.Values)
            {
                handler.Reset();
            }
        }
    }

    [HarmonyPatch(typeof(CardStatistics), nameof(CardStatistics.UpdateStatsForNextTurn))]
    public class CardStatistics_UpdateStatsForNextTurn_Patch
    {
        public static void Postfix()
        {
            foreach (var handler in TrackedValueTypeExtensions.TrackedValueHandlers.Values)
            {
                handler.UpdateStatsForNextTurn();
            }
        }
    }

    [HarmonyPatch(typeof(CardStatistics), nameof(CardStatistics.UpdateStatsForFirstTurn))]
    public class CardStatistics_UpdateStatsForFirstTurn_Patch
    {
        public static void Postfix()
        {
            foreach (var handler in TrackedValueTypeExtensions.TrackedValueHandlers.Values)
            {
                handler.UpdateStatsForFirstTurn();
            }
        }
    }

    [HarmonyPatch(typeof(CardStatistics), nameof(CardStatistics.OnBattleEnd))]
    public class CardStatistics_OnBattleEnd_Patch
    {
        public static void Postfix()
        {
            foreach (var handler in TrackedValueTypeExtensions.TrackedValueHandlers.Values)
            {
                handler.OnBattleEnd();
            }
        }
    }

    [HarmonyPatch(typeof(CardStatistics), nameof(CardStatistics.IncrementStat))]
    public class CardStatistics_IncrementStat_Patch
    {
        // Prefix in case onIncrementedSignal needs the value.
        public static void Prefix(CardState cardState, TrackedValueType trackedValue, int additionalAmount = 1)
        {
            var handler = TrackedValueTypeExtensions.TrackedValueHandlers.GetValueOrDefault(trackedValue);
            if (handler == null)
                return;
            handler.IncrementValue(cardState, additionalAmount);
            handler.IncrementValue(cardState, additionalAmount, EntryDuration.ThisBattle);
        }
    }

    [HarmonyPatch(typeof(CardStatistics), nameof(CardStatistics.GetStatValue))]
    public class CardStatistics_GetStatValue_Patch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "<Pending>")]
        public static bool Prefix(ref int __result, StatValueData statValueData, Dictionary<CardState, CardStatsEntry> ___deckStats, AllGameManagers ___allGameManagers)
        {
            var handler = TrackedValueTypeExtensions.TrackedValueHandlers.GetValueOrDefault(statValueData.trackedValue);
            var getter = TrackedValueTypeExtensions.TrackedValueGetters.GetValueOrDefault(statValueData.trackedValue);
            if (handler == null && getter == null)
                return true;

            if (handler != null)
                __result = handler.GetValue(statValueData);
            else if (getter != null)
                __result = getter.Invoke(statValueData, ___deckStats, ___allGameManagers.GetCoreManagers());
            // skip original
            return false;
        }
    }

    [HarmonyPatch(typeof(CardStatistics), nameof(CardStatistics.GetTrackedValueIsValidOutsideBattle))]
    public class CardStatistics_GetTrackedValueIsValidOutsideBattle_Patch
    {
        public static void Postfix(ref bool __result, TrackedValueType trackedValueType)
        {
            if (TrackedValueTypeExtensions.TrackedValuesValidOutsideBattle.Contains(trackedValueType))
                __result = true;
        }
    }
}
