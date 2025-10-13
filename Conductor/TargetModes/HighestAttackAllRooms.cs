using Conductor.Interfaces;
using ShinyShoe;
using static TargetHelper;

namespace Conductor.TargetModes
{
    /// <summary>
    /// Target Mode that targets the unit with Highest Attack stat on the train (not per floor).
    /// If multiple exists the first in iteration order is selected.
    /// </summary>
    class HighestAttackAllRooms : CharacterTargetSelector
    {
        public override bool TargetsMultipleRooms => true;

        public override void CollectPreviewTargets(CardState? card, RoomManager roomManager, RoomState room, List<CharacterState> previewTargets)
        {
            Team.Type onTeam = Team.Type.Heroes | Team.Type.Monsters;

            for (int i = 0; i < roomManager.GetNumRooms(); i++)
            {
                RoomState room3 = roomManager.GetRoom(i);
                if (room3 != null)
                {
                    room3.AddCharactersToList(previewTargets, onTeam);
                }
            }
        }

        public override bool CollectTargets(CollectTargetsData data, ICoreGameManagers coreGameManagers, List<CharacterState> chosenTargets)
        {
            var roomManager = coreGameManagers.GetRoomManager();
            var heroManager = coreGameManagers.GetHeroManager();
            var monsterManager = coreGameManagers.GetMonsterManager();
            var targetTeamType = data.targetTeamType;

            List<CharacterState> list2;
            using (GenericPools.GetList(out list2))
            {
                for (int num2 = roomManager.GetNumRooms() - 1; num2 >= 0; num2--)
                {
                    if (targetTeamType.HasFlag(Team.Type.Heroes))
                    {
                        heroManager.AddCharactersInRoomToList(list2, num2);
                    }
                    if (targetTeamType.HasFlag(Team.Type.Monsters))
                    {
                        monsterManager.AddCharactersInRoomToList(list2, num2);
                    }
                }
                ApplyTargetFilters(data, list2);
                CharacterState? strongest = null;
                foreach (var current in list2)
                {
                    if (strongest == null || current.GetAttackDamage() > strongest.GetAttackDamage())
                    {
                        strongest = current;
                    }
                }
                if (strongest != null)
                {
                    chosenTargets.Add(strongest);
                }
            }
            // Done processing
            return true;
        }
    }
}
