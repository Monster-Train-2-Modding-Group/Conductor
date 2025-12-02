using Conductor.TrackedValues;
using ShinyShoe;
using System;
using System.Collections.Generic;
using System.Text;
using static CardStatistics;

namespace Conductor.Interfaces
{
    /// <summary>
    /// Main interface for a custom tracked value implementation.
    /// </summary>
    internal interface ITrackedValueHandler
    {
        /// <summary>
        /// Called at the start of a run to completely reset state.
        /// </summary>
        void Reset();
        /// <summary>
        /// Get the value this TrackedValue handles.
        /// </summary>
        /// <param name="statValueData">StatValueData object specifying various parameters, notably the EntryDuration</param>
        /// <returns></returns>
        int GetValue(StatValueData statValueData);
        /// <summary>
        /// Increment the TrackedValue.
        /// </summary>
        /// <param name="card">Card that caused the stat to increment.</param>
        /// <param name="amount">Amount to increase or decrease by</param>
        /// <param name="entryDuration">Optional EntryDuration default is ThisTurn.</param>
        void IncrementValue(CardState? card, int amount, EntryDuration entryDuration = EntryDuration.ThisTurn);
        /// <summary>
        /// Updates the TrackedValue states for the next turn.
        /// Typically you set the stat's value for PreviousTurn to be the CurrentTurn.
        /// And then reset the CurrentTurn stat.
        /// </summary>
        void UpdateStatsForNextTurn();
        /// <summary>
        /// Updates the TrackedValue states for the first turn.
        /// Optimal, can just do nothing, but this function will be called at the start of of a scenario.
        /// </summary>
        void UpdateStatsForFirstTurn();
        /// <summary>
        /// Called when a scenario ends. You should reset the stats.
        /// You can just call Reset here.
        /// </summary>
        void OnBattleEnd();
        /// <summary>
        /// Optional signal for anyone interested in getting a notification when the value changes.
        /// </summary>
        public Signal<TrackedValueChangedParams>? ValueChangedSignal { get; }
    }
}
