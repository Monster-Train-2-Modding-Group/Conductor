using Conductor.Interfaces;
using DG.Tweening.Core.Easing;
using ShinyShoe;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Conductor.TrackedValues
{
    /// <summary>
    /// A Player Resource, similar to DragonsHoard.
    /// 
    /// This class represents a global variable that can be used with the TrackedValue system,
    /// its value is tracked throughout the entire run. The class does not keep track of the
    /// value for the different EntryDurations however. It is just a total of a stat that you have
    /// and can spend. 
    /// 
    /// This class is also Serialiable and must be registered with the SaveDataRegistry after creation.
    /// 
    /// Once associated with a TrackedValue, you should let CardStatistics manage the stat.
    /// If you need to increment the stat you can do so with a call to CardStatistics.IncrementStat.
    /// 
    /// Optionally, but recommended to call CardStatistics.OnPlayerResourceChanged on cards in hand
    /// and Active Monster's spawner cards (see CardManager.OnDragonsHoardChanged / MonsterManager.OnDragonsHoardChanged)
    /// The function is not called for you automatically as it is a coroutine function.
    /// </summary>
    public class PlayerResource : AbstractTrackedValueHandler, ISaveData
    {
        private int current = 0;
        private int preview = 0;
        private readonly ClassData? associatedClass;

        private int ActiveStat => (SaveManager != null && SaveManager.PreviewMode) ? preview : current;
        private readonly string key;
        public string Key => key;

        public PlayerResource(string saveDataKey, ClassData? associatedClass = null)
        {
            this.key = saveDataKey;
            this.associatedClass = associatedClass;
        }

        /// <summary>
        /// Get the value for this tracked value in current game calculating mode.
        /// </summary>
        /// <param name="statValueData">StatValueData object.</param>
        /// <returns>Current value of the tracked value for the specified StatValueData.</returns>
        public override int GetValue(CardStatistics.StatValueData statValueData)
        {
            return ActiveStat;
        }

        /// <summary>
        /// Gets the stat value for ThisTurn.
        /// </summary>
        /// <param name="preview">Get the current stat as of a game preview instead.</param>
        /// <returns>The current stat value for this turn.</returns>
        public int GetValue(bool previewValue = false)
        {
            if (previewValue)
                return preview;
            else
                return current;
        }

        /// <summary>
        /// Increment the stat value. This function is not intended to be called directly.
        /// If you do then be sure to call the function both EntryDuration.ThisTurn and ThisBattle.
        /// </summary>
        public override void IncrementValue(CardState? card, int amount, CardStatistics.EntryDuration entryDuration = CardStatistics.EntryDuration.ThisTurn)
        {
            if (entryDuration != CardStatistics.EntryDuration.ThisTurn) 
                return;

            int previous = ActiveStat;
            if (SaveManager != null && SaveManager.PreviewMode)
            {
                preview += amount;
            }
            else
            {
                current += amount;
                preview = current;
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
            if (entryDuration != CardStatistics.EntryDuration.ThisTurn)
                return;
            int previous = ActiveStat;
            if (SaveManager != null && SaveManager.PreviewMode)
            {
                preview = amount;
            }
            else
            {
                current = amount;
                preview = current;
                ValueChanged(current, previous, card, entryDuration);
            }
        }

        public override void OnBattleEnd()
        {
        }

        public override void Reset()
        {
            current = 0;
            preview = 0;
            ValueChanged(current);
        }

        public override void UpdateStatsForNextTurn()
        {
        }

        public override void OnCombatPreviewEnabled()
        {
            preview = current;
        }

        public override void OnCombatPreviewDisabled()
        {
            preview = current;
        }

        public virtual string Serialize()
        {
            return current.ToString();
        }

        public virtual void Deserialize(string data)
        {
            current = 0;
            Int32.TryParse(data, out current);
            ValueChanged(current);
        }

        public virtual bool ShouldSerialize()
        {
            if (associatedClass == null)
                return true;

            if (SaveManager!.GetMainClass() != associatedClass && SaveManager.GetSubClass() != associatedClass)
                return false;

            return true;
        }
    }
}
