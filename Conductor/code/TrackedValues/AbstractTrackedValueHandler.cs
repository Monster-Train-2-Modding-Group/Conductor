using Conductor.Interfaces;
using ShinyShoe;
using System;
using System.Collections.Generic;
using System.Text;
using static CardStatistics;
using static Conductor.TrackedValues.TrackedValueChangedParams;

namespace Conductor.TrackedValues
{
    /// <summary>
    /// Main class for a custom tracked value implementation.
    /// 
    /// Note that if your TrackedValue is associated with a card you need not make a class as
    /// the default behavior of a TrackedValue is to track a particular stat about a card.
    /// </summary>
    public abstract class AbstractTrackedValueHandler : ITrackedValueHandler
    {
        /// <summary>
        /// Optional signal for anyone interested in getting a notification when the value changes.
        /// </summary>
        protected Signal<TrackedValueChangedParams> valueChangedSignal;
        /// <summary>
        /// Signal for any listeners interested in subscribing.
        /// </summary>
        public Signal<TrackedValueChangedParams> ValueChangedSignal => valueChangedSignal;
        /// <summary>
        /// Default constructor calls Reset and creates the Value changed signal.
        /// </summary>
        public AbstractTrackedValueHandler()
        {
            valueChangedSignal = new Signal<TrackedValueChangedParams>();
            Reset();
        }
        /// <summary>
        /// Get the value this TrackedValue handles.
        /// </summary>
        /// <param name="statValueData">StatValueData object specifying various parameters, notably the EntryDuration</param>
        /// <returns>The current value of the TrackedValue for the given StatValueData</returns>
        public abstract int GetValue(CardStatistics.StatValueData statValueData);
        /// <summary>
        /// Increment the TrackedValue, and dispatch the signal.
        /// </summary>
        /// <param name="card">Card that caused the stat to increment.</param>
        /// <param name="amount">Amount to increase or decrease by</param>
        /// <param name="entryDuration">Optional EntryDuration default is ThisTurn.</param>
        public abstract void IncrementValue(CardState? card, int amount, CardStatistics.EntryDuration entryDuration = CardStatistics.EntryDuration.ThisTurn);
        /// <summary>
        /// Called when a scenario ends. You should reset the stats if the TrackedValue doesn't carryover between battles.
        /// The default implementation calls Reset().
        /// </summary>
        public virtual void OnBattleEnd()
        {
            Reset();
        }
        /// <summary>
        /// Called at the start of a run to completely reset state.
        /// </summary>
        public virtual void Reset()
        {
        }
        /// <summary>
        /// Updates the TrackedValue states for the first turn.
        /// Optimal, can just do nothing, but this function will be called at the start of of a scenario.
        /// </summary>
        public virtual void UpdateStatsForFirstTurn()
        {
        }
        /// <summary>
        /// Updates the TrackedValue states for the next turn.
        /// Typically you set the stat's value for PreviousTurn to be the CurrentTurn.
        /// And then reset the CurrentTurn stat.
        /// </summary>
        public virtual void UpdateStatsForNextTurn()
        {
        }
        /// <summary>
        /// Optional Helper function to Dispatch the signal call when the current value is directly modified.
        /// </summary>
        /// <param name="currentValue">Current value of the tracked value for entry duration.</param>
        /// <param name="previousValue">Previous value of the tracked value for entry duration.</param>
        /// <param name="card">Associated Card.</param>
        /// <param name="entryDuration">Entry Duration</param>
        protected void ValueChanged(int currentValue, int? previousValue = null, CardState? card = null, EntryDuration entryDuration = EntryDuration.ThisTurn, UiUpdateMode updateUI = UiUpdateMode.Animated)
        {
            valueChangedSignal.Dispatch(new TrackedValueChangedParams { value = currentValue, previousValue = previousValue, card = card, entryDuration = entryDuration, updateUI = updateUI });
        }
    }
}
