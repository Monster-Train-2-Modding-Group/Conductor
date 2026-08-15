using HarmonyLib;
using ShinyShoe;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static CardStatistics;

namespace Conductor.TrackedValues
{
    public static class TrackedValueFunctions
    {
        private static bool CheckSubtypeMatch(CardState card, SubtypeData subtype, RelicManager relicManager)
        {
            if (card != null && subtype != null)
            {
                CharacterData? spawnCharacterData = card.GetSpawnCharacterData();
                bool flag = relicManager.GetRelicEffect<RelicEffectMonstersAreAllSubtypes>() != null && !subtype.IsChampion;
                if (spawnCharacterData != null)
                {
                    List<SubtypeData> subtypes = spawnCharacterData.GetSubtypes();
                    if (flag || (subtypes != null && subtypes.Contains(subtype)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

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

        internal static int CountUnitsInTargetRoom(CardStatistics.StatValueData statValueData, IReadOnlyDictionary<CardState, CardStatsEntry> deckStats, ICoreGameManagers coreGameManagers)
        {
            var roomManager = coreGameManagers.GetRoomManager();
            var relicManager = coreGameManagers.GetRelicManager();
            var room = roomManager?.GetRoom(roomManager.GetSelectedRoom());
            if (room == null)
            {
                return 0;
            }

            int count = 0;
            Team.Type team = statValueData.paramTeamType;
            using (GenericPools.GetList(out List<CharacterState> list))
            {
                room.AddCharactersToList(list, team);
                count = CountMatchingCharactersInList(statValueData, relicManager, list);
            }

            return count;
        }

        internal static int CountUnitsOnTrain(CardStatistics.StatValueData statValueData, IReadOnlyDictionary<CardState, CardStatsEntry> deckStats, ICoreGameManagers coreGameManagers)
        {
            var roomManager = coreGameManagers.GetRoomManager();
            var relicManager = coreGameManagers.GetRelicManager();
            var heroManager = coreGameManagers.GetHeroManager();
            var monsterManager = coreGameManagers.GetMonsterManager();

            var room = roomManager?.GetRoom(roomManager.GetSelectedRoom());
            if (room == null)
                return 0;

            int count = 0;
            Team.Type team = statValueData.paramTeamType;
            using (GenericPools.GetList(out List<CharacterState> list))
            {
                if (team.HasFlag(Team.Type.Heroes))
                {
                    heroManager.AddCharactersInTowerToList(list);
                }
                if (team.HasFlag(Team.Type.Monsters))
                {
                    monsterManager.AddCharactersInTowerToList(list);
                }
                count = CountMatchingCharactersInList(statValueData, relicManager, list);
            }

            return count;
        }

        private static int CountMatchingCharactersInList(StatValueData statValueData, RelicManager relicManager, List<CharacterState> list)
        {
            int count = 0;
            CardData paramCardData = statValueData.paramCardData;
            CardTypeTarget cardTypeTarget = statValueData.cardTypeTarget;
            SubtypeData paramSubtype = statValueData.paramSubtype;
            StatusEffectStackData[] paramStatusEffects = statValueData.paramStatusEffects;
            CardUpgradeMaskData cardFilter = statValueData.cardFilter;

            foreach (var character in list)
            {
                if (paramCardData != null && character.GetSpawnerCard().GetCardDataID() != paramCardData?.GetID())
                {
                    continue;
                }
                if (!paramStatusEffects.IsNullOrEmpty())
                {
                    bool invalid = false;
                    foreach (var statusEffect in paramStatusEffects)
                    {
                        if (character.GetStatusEffectStacks(statusEffect.statusId) < statusEffect.count)
                        {
                            invalid = true;
                            break;
                        }
                    }
                    if (invalid)
                        continue;
                }
                if (paramSubtype != null && !paramSubtype.IsNone && !CheckSubtypeMatch(character.GetSpawnerCard(), paramSubtype, relicManager))
                {
                    continue;
                }
                if (cardFilter != null && !cardFilter.FilterCharacter(character, relicManager))
                {
                    continue;
                }
                count++;
            }

            return count;
        }
    }
}
