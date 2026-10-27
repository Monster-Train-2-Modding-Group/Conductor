using static CardStatistics;

namespace Conductor.TrackedValues
{
    /// <summary>
    /// Tracked Value Handler that aggregrates its value across all specific cards.
    /// </summary>
    public class CardAggregateTrackedValueHandler : AbstractTrackedValueHandler
    {
        private readonly Dictionary<string, int[]> currentStats = [];
        private readonly Dictionary<string, int[]> previewStats = [];

        private void EnsureAllocated(string cardDataId)
        {
            if (!currentStats.ContainsKey(cardDataId))
                currentStats[cardDataId] = new int[3];
            if (!previewStats.ContainsKey(cardDataId))
                previewStats[cardDataId] = new int[3];
        }

        private int[] ActiveStats(string cardDataId)
        {
            EnsureAllocated(cardDataId);
            var dict = (SaveManager != null && SaveManager.PreviewMode) ? previewStats : currentStats;
            return dict[cardDataId];
        }

        private int[] ActiveStats(CardData data) => ActiveStats(data.GetID());
        private int[] ActiveStats(CardState data) => ActiveStats(data.GetCardDataID());

        public override int GetValue(CardStatistics.StatValueData statValueData)
        {
            int index = (int)statValueData.entryDuration;
            var card = statValueData.paramCardData;
            if (card == null)
            {
                Plugin.Logger.LogError($"Card {statValueData.cardState} Attempted to fetch a CardAggregateTrackedValueHandler without setting paramCardData");
                return 0;
            }
            return ActiveStats(card).ElementAtOrDefault(index);
        }

        public override void IncrementValue(CardState? card, int amount, CardStatistics.EntryDuration entryDuration = CardStatistics.EntryDuration.ThisTurn)
        {
            // Not implemented
            Plugin.Logger.LogError($"Card {card} Attempted to increment a CardAggregateTrackedValueHandler");
        }

        public override void OnTrackedValueIncremented(CardState? card, TrackedValueType trackedValue, int additionalAmount)
        {
            if (card == null)
                return;

            var stats = ActiveStats(card);
            stats[(int)CardStatistics.EntryDuration.ThisTurn] += additionalAmount;
            stats[(int)CardStatistics.EntryDuration.ThisBattle] += additionalAmount;
        }

        public override void OnBattleEnd()
        {
            DoReset(true);
        }

        public override void Reset()
        {
            DoReset(false);
        }

        public void DoReset(bool hardReset = true)
        {
            if (hardReset)
            {
                currentStats.Clear();
                previewStats.Clear();
            }
            else
            {
                foreach (var card in currentStats.Keys)
                {
                    currentStats[card][0] = currentStats[card][1] = currentStats[card][2] = 0;
                    previewStats[card][0] = previewStats[card][1] = previewStats[card][2] = 0;
                }
            }
        }

        public override void UpdateStatsForNextTurn()
        {
            foreach (var card in currentStats.Keys)
            {
                var current = currentStats[card];
                var preview = previewStats[card];

                int previousTurn = current[(int)CardStatistics.EntryDuration.PreviousTurn] = current[(int)CardStatistics.EntryDuration.ThisTurn];
                int thisTurn = current[(int)CardStatistics.EntryDuration.ThisTurn] = 0;
                preview[(int)CardStatistics.EntryDuration.PreviousTurn] = previousTurn;
                preview[(int)CardStatistics.EntryDuration.ThisTurn] = 0;
            }
        }

        public override void OnCombatPreviewEnabled()
        {
            foreach (var card in currentStats.Keys)
            {
                var current = currentStats[card];
                var preview = previewStats[card];
                for (int i = 0; i < preview.Length; i++)
                {
                    preview[i] = current[i];
                }
            }
        }

        public override void OnCombatPreviewDisabled()
        {
            foreach (var card in previewStats.Keys)
            {
                var current = currentStats[card];
                var preview = previewStats[card];
                for (int i = 0; i < preview.Length; i++)
                {
                    preview[i] = current[i];
                }
            }
        }
    }
}
