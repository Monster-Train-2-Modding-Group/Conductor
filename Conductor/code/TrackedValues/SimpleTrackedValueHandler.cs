using Conductor.Interfaces;
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
    public class SimpleGlobalTrackedValueHandler : ITrackedValueHandler
    {
        private int thisTurn;
        private int previousTurn;
        private int thisBattle;

        public int GetValue(CardStatistics.StatValueData statValueData)
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
        /// 
        /// </summary>
        public void IncrementValue(CardState? _, int amount, CardStatistics.EntryDuration entryDuration = CardStatistics.EntryDuration.ThisTurn)
        {
            switch (entryDuration)
            {
                case CardStatistics.EntryDuration.ThisTurn:
                    thisTurn += amount;
                    break;
                case CardStatistics.EntryDuration.PreviousTurn:
                    previousTurn += amount;
                    break;
                case CardStatistics.EntryDuration.ThisBattle:
                    thisBattle += amount;
                    break;
            }
        }

        /// <summary>
        /// Helper function to directly modify the stat
        /// </summary>
        /// <param name="amount">Set the current value for EntryDuration.</param>
        /// <param name="entryDuration">EntryDuration to modify.</param>
        public void SetValue(CardState? _, int amount, CardStatistics.EntryDuration entryDuration = CardStatistics.EntryDuration.ThisTurn)
        {
            switch (entryDuration)
            {
                case CardStatistics.EntryDuration.ThisTurn:
                    thisTurn = amount;
                    break;
                case CardStatistics.EntryDuration.PreviousTurn:
                    previousTurn = amount;
                    break;
                case CardStatistics.EntryDuration.ThisBattle:
                    thisBattle = amount;
                    break;
            }
        }

        public void OnBattleEnd()
        {
            Reset();
        }

        public void Reset()
        {
            thisTurn = 0;
            previousTurn = 0;
            thisBattle = 0;
        }

        public void UpdateStatsForNextTurn()
        {
            previousTurn = thisTurn;
            thisTurn = 0;
        }

        public void UpdateStatsForFirstTurn()
        {
        }
    }
}
