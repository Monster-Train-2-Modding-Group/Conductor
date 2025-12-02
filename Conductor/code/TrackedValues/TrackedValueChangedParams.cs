using static CardStatistics;

namespace Conductor.TrackedValues
{
    /// <summary>
    /// Signal parameters for when this tracked value changes.
    /// </summary>
    public struct TrackedValueChangedParams
    {
        public enum UiUpdateMode
        {
            NoRefresh = 0,
            Instant = 1,
            Animated = 2,
        }
        /// <summary>
        /// Current value
        /// </summary>
        public int value;
        /// <summary>
        /// Previous value (Optional).
        /// </summary>
        public int? previousValue;
        /// <summary>
        /// If the tracked value is associated with a card the associated card (Optional).
        /// </summary>
        public CardState? card;
        /// <summary>
        /// The entry duration that was changed.
        /// </summary>
        public EntryDuration entryDuration;
        /// <summary>
        /// How to update the UI.
        /// </summary>
        public UiUpdateMode updateUI = UiUpdateMode.Animated;

        public TrackedValueChangedParams()
        {
        }
    }
}
