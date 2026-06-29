using Conductor.Interfaces;
using ShinyShoe;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Conductor.TrackedValues
{
    /// <summary>
    /// Simple TrackedValue class that implements ITrackedValueHandler
    /// 
    /// This class represents a simple global variable that is valid in-battle that can be
    /// used with the TrackedValue system.
    /// 
    /// The class manages the stat for all EntryDurations, you can get the stats value as of the current turn,
    /// previous turn and the entire battle.
    /// 
    /// Once associated with a TrackedValue, you should let CardStatistics manage the stat so that the internal
    /// tracking updates properly. If you need to increment the stat you should do so with a call to 
    /// CardStatistics.IncrementStat.
    /// </summary>
    public class SimpleGlobalTrackedValueHandler : AbstractTrackedValueHandler
    {
        private readonly int[] currentStats = new int[3];
        private readonly int[] previewStats = new int[3];

        private int[] ActiveStats => (SaveManager != null && SaveManager.PreviewMode) ? previewStats : currentStats;

        /// <summary>
        /// Get the value for this tracked value in current game calculating mode.
        /// </summary>
        /// <param name="statValueData">StatValueData object.</param>
        /// <returns>Current value of the tracked value for the specified StatValueData.</returns>
        public override int GetValue(CardStatistics.StatValueData statValueData)
        {
            int index = (int)statValueData.entryDuration;
            return ActiveStats.ElementAtOrDefault(index);
        }

        /// <summary>
        /// Gets the stat value for ThisTurn.
        /// </summary>
        /// <param name="preview">Get the current stat as of a game preview instead.</param>
        /// <returns>The current stat value for this turn.</returns>
        public int GetCurrentValue(bool preview = false)
        {
            if (preview)
                return previewStats[0];
            else
                return currentStats[0];
        }

        /// <summary>
        /// Gets the stat value for PreviousTurn.
        /// </summary>
        /// <param name="preview">Get the previous turn stat as of a game preview instead.</param>
        /// <returns>The current stat value for the previous turn.</returns>
        public int GetPreviousValue(bool preview = false)
        {
            if (preview)
                return previewStats[1];
            else
                return currentStats[1];
        }

        /// <summary>
        /// Gets the stat value for ThisBattle.
        /// </summary>
        /// <param name="preview">Get the this battle turn stat as of a game preview instead.</param>
        /// <returns>The this battle stat value for the previous turn.</returns>
        public int GetTotalValue(bool preview = false)
        {
            if (preview)
                return previewStats[2];
            else
                return currentStats[2];
        }

        /// <summary>
        /// Increment the stat value. This function is not intended to be called directly.
        /// If you do then be sure to call the function both EntryDuration.ThisTurn and ThisBattle.
        /// </summary>
        public override void IncrementValue(CardState? card, int amount, CardStatistics.EntryDuration entryDuration = CardStatistics.EntryDuration.ThisTurn)
        {
            int index = (int)entryDuration;
            if (index < 0 || index > 2) return;
            int[] stats = ActiveStats;

            int previous = stats[index];
            stats[index] += amount;
            int current = stats[index];

            if (SaveManager != null && !SaveManager.PreviewMode)
            {
                previewStats[index] = current;
                ValueChanged(current, previous, card, entryDuration);
            }
        }

        /// <summary>
        /// Helper function to directly modify the stat
        /// </summary>
        /// <param name="amount">Set the current value for EntryDuration.</param>
        /// <param name="entryDuration">EntryDuration to modify.</param>
        public void SetValue(CardState? card, int amount, CardStatistics.EntryDuration entryDuration = CardStatistics.EntryDuration.ThisTurn, bool notify = true)
        {
            int index = (int)entryDuration;
            if (index < 0 || index > 2) return;
            int[] stats = ActiveStats;

            int current = amount;
            int previous = stats[index];
            stats[index] = amount;

            if (notify && SaveManager != null && !SaveManager.PreviewMode)
            {
                previewStats[index] = current;
                ValueChanged(current, previous, card, entryDuration);
            }   
        }

        public override void OnBattleEnd()
        {
            Reset();
        }

        public override void Reset()
        {
            for (int i = 0; i < ActiveStats.Length; i++)
            {
                ActiveStats[i] = 0;
            }
            if (SaveManager != null && !SaveManager.PreviewMode)
            {
                ValueChanged(0, 0, updateUI: TrackedValueChangedParams.UiUpdateMode.Instant);
            }    
        }

        public override void UpdateStatsForNextTurn()
        {
            int previousTurn = currentStats[(int) CardStatistics.EntryDuration.PreviousTurn] = currentStats[(int) CardStatistics.EntryDuration.ThisTurn];
            int thisTurn = currentStats[(int)CardStatistics.EntryDuration.ThisTurn] = 0;
            previewStats[(int)CardStatistics.EntryDuration.PreviousTurn] = previousTurn;
            previewStats[(int)CardStatistics.EntryDuration.ThisTurn] = 0;
            ValueChanged(thisTurn, previousTurn, updateUI: TrackedValueChangedParams.UiUpdateMode.Instant);
        }

        public override void OnCombatPreviewEnabled()
        {
            for (int i = 0; i < previewStats.Length; i++)
            {
                previewStats[i] = currentStats[i];
            }
        }

        public override void OnCombatPreviewDisabled()
        {
            for (int i = 0; i < previewStats.Length; i++)
            {
                previewStats[i] = currentStats[i];
            }
        }
    }
}
