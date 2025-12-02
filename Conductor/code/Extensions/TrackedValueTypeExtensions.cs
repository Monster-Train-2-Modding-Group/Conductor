using Conductor.TrackedValues;

namespace Conductor.Extensions
{
    public delegate int TrackedValueGetter(CardStatistics.StatValueData statValueData, IReadOnlyDictionary<CardState, CardStatsEntry> deckStats, ICoreGameManagers coreGameManagers);

    public static class TrackedValueTypeExtensions
    {
        
        internal readonly static Dictionary<CardStatistics.TrackedValueType, AbstractTrackedValueHandler> TrackedValueHandlers = [];
        internal readonly static Dictionary<CardStatistics.TrackedValueType, TrackedValueGetter> TrackedValueGetters = [];
        internal readonly static ISet<CardStatistics.TrackedValueType> TrackedValuesValidOutsideBattle = new HashSet<CardStatistics.TrackedValueType>();

        private static bool IsVanillaTrackedValue(CardStatistics.TrackedValueType trackedValue)
        {
            if ((int)trackedValue <= (from int x in Enum.GetValues(typeof(CardStatistics.TrackedValueType)).AsQueryable() select x).Max())
            {
                Plugin.Logger.LogError($"Attempt to redefine vanilla trackedValue {trackedValue}, you probably didn't mean to do this?");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Associates a Custom TrackedValue with a handler class.
        /// 
        /// Implement a class deriving from ITrackedValueHandler and pass an instance to this function.
        /// Or use pass an instance SimpleGlobalTrackedValueHandler if the TrackedValue is globally known value to be used in CardTraits/RelicEffectConditions.
        /// 
        /// Do not use this function if your TrackedValue is associated with a CardState. That is the default behavior can you can just use
        /// CardStatistics out of the box like any other TrackedValue.
        /// </summary>
        /// <param name="trackedValue">A custom TrackedValue enum</param>
        /// <param name="handler">Handler class for the TrackedValue.</param>
        /// <returns>The TrackedValue</returns>
        public static CardStatistics.TrackedValueType SetTrackedValueHandler(this CardStatistics.TrackedValueType trackedValue, AbstractTrackedValueHandler handler)
        {
            if (IsVanillaTrackedValue(trackedValue)) return trackedValue;
            if (TrackedValueGetters.ContainsKey(trackedValue))
            {
                Plugin.Logger.LogError($"Custom TrackedValue {trackedValue} is already associated with a Getter function. Ignoring...");
                Plugin.Logger.LogDebug(Environment.StackTrace);
                return trackedValue;
            }
            TrackedValueHandlers.Add(trackedValue, handler);
            return trackedValue;
        }

        /// <summary>
        /// Associate a Custom TrackedValue with a Getter function that computes its value.
        /// 
        /// This is useful for simpler scenarios like a TrackedValue that returns the current
        /// number of cards of a particular type in deck.
        /// 
        /// The getter can be EntryDuration aware as well, but if you need notifications on when the turn changes
        /// Then its best to write a full class implementing from ITrackedValueHandler instead.
        /// </summary>
        /// <param name="trackedValue">A custom TrackedValue enum</param>
        /// <param name="handler">Handler function for the TrackedValue. Takes a StatValueData and ICoreGameManagers and returns an int for the TrackedValue.</param>
        /// <returns>The TrackedValue</returns>
        public static CardStatistics.TrackedValueType SetTrackedValueGetter(this CardStatistics.TrackedValueType trackedValue, TrackedValueGetter handler)
        {
            if (IsVanillaTrackedValue(trackedValue)) return trackedValue;
            if (TrackedValueHandlers.ContainsKey(trackedValue))
            {
                Plugin.Logger.LogError($"Custom TrackedValue {trackedValue} is already associated with a Handler class. Ignoring...");
                Plugin.Logger.LogDebug(Environment.StackTrace);
                return trackedValue;
            }
            TrackedValueGetters.Add(trackedValue, handler);
            return trackedValue;
        }

        /// <summary>
        /// Allows the TrackedValue to be displayed on cards outside battle.
        /// 
        /// By default TrackedValues are only valid within a battle, so calling this on the TrackedValue makes it valid outside battle.
        /// </summary>
        /// <param name="trackedValue">Custom TrackedValue</param>
        /// <returns>The TrackedValue</returns>
        public static CardStatistics.TrackedValueType SetIsValidOutsideBattle(this CardStatistics.TrackedValueType trackedValue)
        {
            if (IsVanillaTrackedValue(trackedValue)) return trackedValue;
            TrackedValuesValidOutsideBattle.Add(trackedValue);
            return trackedValue;
        }
    }
}
