using System.Collections;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Card effect that shuffles the Team.Type specified.
    /// 
    /// Test fails if there's no shuffling (Heroes and Monsters count on the floor needs to be <1 for each)
    /// 
    /// Example Json.
    /// "effects": [
    ///   {
    ///     "id": "ShuffleEveryone",
    ///     "name": {
    ///       "id": "@CardEffectShuffleUnits",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "target_mode": "room",
    ///     "target_team": "both",
    ///   }
    /// ]
    /// </summary>
    class CardEffectShuffleUnits : CardEffectBase, ICardEffectChangesUnitPosition
    {
        public override bool CanPlayAfterBossDead => false;
        public override bool CanApplyInPreviewMode => false;
        private readonly List<CharacterState> allTargets = [];
        private readonly List<CharacterState> shuffledUnits = [];

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return [];
        }

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            bool flag = false;
            RoomState? roomState = cardEffectParams.GetSelectedRoom(coreGameManagers.GetRoomManager());
            if (cardEffectState.GetTargetTeamType().HasFlag(Team.Type.Monsters))
            {
                allTargets.Clear();
                roomState?.AddMovableCharactersToList(allTargets, Team.Type.Monsters);
                flag = allTargets.Count > 1;
            }
            if (!flag && cardEffectState.GetTargetTeamType().HasFlag(Team.Type.Heroes))
            {
                allTargets.Clear();
                roomState?.AddMovableCharactersToList(allTargets, Team.Type.Heroes);
                flag = allTargets.Count > 1;
            }
            return flag;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers systemManagers)
        {
            RoomState? roomState = cardEffectParams.GetSelectedRoom(coreGameManagers.GetRoomManager());
            if (roomState != null)
            {
                if (cardEffectState.GetTargetTeamType().HasFlag(Team.Type.Monsters))
                {
                    allTargets.Clear();
                    roomState.AddMovableCharactersToList(allTargets, Team.Type.Monsters);
                    if (allTargets.Count > 1)
                    {
                        yield return ShuffleUnitsInRoom(allTargets, roomState, coreGameManagers.GetSaveManager().GetActiveTiming().MovementTime, coreGameManagers.GetRelicManager(), coreGameManagers.GetHeroManager());
                    }
                }
                if (cardEffectState.GetTargetTeamType().HasFlag(Team.Type.Heroes))
                {
                    allTargets.Clear();
                    roomState.AddMovableCharactersToList(allTargets, Team.Type.Heroes);
                    if (allTargets.Count > 1)
                    {
                        yield return ShuffleUnitsInRoom(allTargets, roomState, coreGameManagers.GetSaveManager().GetActiveTiming().MovementTime, coreGameManagers.GetRelicManager(), coreGameManagers.GetHeroManager());
                    }
                }
            }
        }

        private IEnumerator ShuffleUnitsInRoom(List<CharacterState> units, RoomState roomState, float movementTime, RelicManager relicManager, HeroManager heroManager)
        {
            shuffledUnits.Clear();
            for (int i = 0; i < units.Count; i++)
            {
                CharacterState characterState = units[i];
                if (!shuffledUnits.Contains(characterState))
                {
                    CharacterState characterState2 = characterState;
                    while (characterState2 == characterState)
                    {
                        characterState2 = units[RandomManager.Range(0, units.Count, RngId.Battle)];
                    }
                    SpawnPoint spawnPoint = characterState.GetSpawnPoint();
                    characterState.SetSpawnPoint(characterState2.GetSpawnPoint(), animate: true);
                    characterState2.SetSpawnPoint(spawnPoint, animate: true);
                    shuffledUnits.Add(characterState);
                    shuffledUnits.Add(characterState2);
                }
            }
            roomState.UpdateSpawnPointPositions();
            yield return CoreUtil.WaitForSecondsOrBreak(movementTime);
            foreach (CharacterState unit in units)
            {
                yield return relicManager.CharacterFloorRearranged(unit);
            }
            foreach (CharacterState shuffledUnit in shuffledUnits)
            {
                yield return heroManager.ShiftCharacterTrigger(shuffledUnit);
            }
        }
    }
}
