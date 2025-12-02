using Conductor.Interfaces;
using ShinyShoe;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.TrackedValues
{
    /// <summary>
    /// Simple TrackedValue class that implements ITrackedValueHandler
    /// 
    /// This class represents a simple global variable that can be used with the
    /// TrackedValue system.
    /// 
    /// Once associated with a TrackedValue, you should let CardStatistics manage the stat.
    /// If you need to increment the stat you can do so with a call to CardStatistics.IncrementStat.
    /// 
    /// </summary>
    public class SimpleGlobalTrackedValueHandler : AbstractTrackedValueHandler
    {
        private int thisTurn;
        private int previousTurn;
        private int thisBattle;

        /// <summary>
        /// Get the value for this tracked value.
        /// </summary>
        /// <param name="statValueData">StatValueData object.</param>
        /// <returns>Current value of the tracked value for the specified StatValueData.</returns>
        public override int GetValue(CardStatistics.StatValueData statValueData)
        {
            return statValueData.entryDuration switch
            {
                CardStatistics.EntryDuration.ThisTurn => thisTurn,
                CardStatistics.EntryDuration.PreviousTurn => previousTurn,
                CardStatistics.EntryDuration.ThisBattle => thisBattle,
                _ => 0,
            };
        }

        /// <summary>
        /// Increment the stat value. This function is not intended to be called directly.
        /// If you do then be sure to call the function both EntryDuration.ThisTurn and ThisBattle.
        /// </summary>
        public override void IncrementValue(CardState? card, int amount, CardStatistics.EntryDuration entryDuration = CardStatistics.EntryDuration.ThisTurn)
        {
            int previous;
            int current;
            switch (entryDuration)
            {
                case CardStatistics.EntryDuration.ThisTurn:
                    previous = thisTurn;
                    thisTurn += amount;
                    current = thisTurn;
                    break;
                case CardStatistics.EntryDuration.PreviousTurn:
                    previous = previousTurn;
                    previousTurn += amount;
                    current = previousTurn;
                    break;
                case CardStatistics.EntryDuration.ThisBattle:
                    previous = thisBattle;
                    thisBattle += amount;
                    current = thisBattle;
                    break;
                default:
                    return;
            }
            ValueChanged(current, previous, card, entryDuration);
        }

        /// <summary>
        /// Helper function to directly modify the stat
        /// </summary>
        /// <param name="amount">Set the current value for EntryDuration.</param>
        /// <param name="entryDuration">EntryDuration to modify.</param>
        public void SetValue(CardState? card, int amount, CardStatistics.EntryDuration entryDuration = CardStatistics.EntryDuration.ThisTurn, bool notify = true)
        {
            int current = amount;
            int previous;
            switch (entryDuration)
            {
                case CardStatistics.EntryDuration.ThisTurn:
                    previous = thisTurn;
                    thisTurn = amount;
                    break;
                case CardStatistics.EntryDuration.PreviousTurn:
                    previous = previousTurn;
                    previousTurn = amount;
                    break;
                case CardStatistics.EntryDuration.ThisBattle:
                    previous = thisBattle;
                    thisBattle = amount;
                    break;
                default:
                    return;
            }
            if (notify)
                ValueChanged(current, previous, card, entryDuration);
        }

        public override void OnBattleEnd()
        {
            Reset();
        }

        public override void Reset()
        {
            thisTurn = 0;
            previousTurn = 0;
            thisBattle = 0;
            ValueChanged(thisTurn, previousTurn, updateUI: TrackedValueChangedParams.UiUpdateMode.Instant);
        }

        public override void UpdateStatsForNextTurn()
        {
            previousTurn = thisTurn;
            thisTurn = 0;
            ValueChanged(thisTurn, previousTurn, updateUI: TrackedValueChangedParams.UiUpdateMode.Instant);
        }
    }
}
