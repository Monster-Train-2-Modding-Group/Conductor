using ShinyShoe;
using UnityEngine.Pool;
using static TargetHelper;

namespace Conductor.TargetModes
{
    /// <summary>
    /// Target Mode that targets the back unit on each room.
    /// </summary>
    class BackInAllRooms : CharacterTargetSelector
    {
        public override bool TargetsMultipleRooms => true;
        public override CardTargetMode CardTargetMode { get; } = CardTargetMode.Other;

        public override void CollectPreviewTargets(CardState? card, RoomManager roomManager, RoomState room, List<CharacterState> previewTargets)
        {
            for (int i = 0; i < roomManager.GetNumRooms(); i++)
            {
                roomManager.GetRoom(i)?.AddCharactersToList(previewTargets, Team.Type.Heroes | Team.Type.Monsters);
            }
        }

        public override bool CollectTargets(CollectTargetsData data, ICoreGameManagers coreGameManagers, List<CharacterState> targetsOut)
        {
            var roomManager = coreGameManagers.GetRoomManager();
            var heroManager = coreGameManagers.GetHeroManager();
            var monsterManager = coreGameManagers.GetMonsterManager();
            var includeUntouchable = data.includeUntouchable;
            using (GenericPools.GetList<CharacterState>(out var list2))
            {
                var targetTeamType = data.targetTeamType;
                for (int m = 0; m < roomManager.GetNumUpgradableRooms(); m++)
                {
                    if (targetTeamType.HasFlag(Team.Type.Heroes))
                    {
                        heroManager.AddCharactersInRoomToList(list2, m);
                    }
                    if (targetTeamType.HasFlag(Team.Type.Monsters))
                    {
                        monsterManager.AddCharactersInRoomToList(list2, m);
                    }
                }
                foreach (CharacterState item in list2)
                {
                    if (IsInBackOfRoom(roomManager, item, includeUntouchable))
                    {
                        targetsOut.Add(item);
                    }
                }
            }
            return true;
        }

        private bool IsInBackOfRoom(RoomManager roomManager, CharacterState character, bool includeUntouchable)
        {
            RoomState room = roomManager.GetRoom(character.GetCurrentRoomIndex());
            using (GenericPools.GetList<CharacterState>(out var list))
            {
                room.AddCharactersToList(list, character.GetTeamType());
                if (!includeUntouchable)
                {
                    list.RemoveAll((CharacterState i) => i != null && i.HasStatusEffect("untouchable"));
                }
                return list.Count != 0 && list.LastIndexOf(character) == list.Count - 1;
            }
        }

    }
}
