using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.TrackedValues
{
    public static class TrackedValueFunctions
    {
        internal static int CountBlightsAndScourgesInDeck(CardStatistics.StatValueData _, IReadOnlyDictionary<CardState, CardStatsEntry> deckStats, ICoreGameManagers coreGameManagers)
        {
            int count = 0;
            var cardManager = coreGameManagers.GetCardManager();
            foreach (CardState card in deckStats.Keys)
            {
                if (cardManager != null && (cardManager.GetExhaustedPile().Contains(card) || cardManager.GetEatenPile().Contains(card) || cardManager.GetPurgedPile().Contains(card)))
                {
                    continue;
                }
                if (card.GetCardType() == CardType.Blight || card.GetCardType() == CardType.Junk)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
