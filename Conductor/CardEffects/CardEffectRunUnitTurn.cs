using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Version of CardEffectAttackWithUnit that allows Hero units to attack.
    /// Using the Vanilla version with an enemy unit as a target leads to an enemy unit attacking the front enemy unit (or whatever) in a room.
    /// 
    /// Example Json.
    /// "effects": [
    ///   {
    ///     "id": "FrontEnemyUnitAttacksImmediately",
    ///     "name": {
    ///       "id": "@CardEffectRunUnitTurn",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "target_mode": "front_in_room",
    ///     "target_team": "heroes"
    ///   }
    /// ]
    /// </summary>
    public class CardEffectRunUnitTurn : CardEffectBase
    {
        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            CombatManager combatManager = coreGameManagers.GetCombatManager();
            MonsterManager monsterManager = coreGameManagers.GetMonsterManager();
            HeroManager heroManager = coreGameManagers.GetHeroManager();
            if (cardEffectParams.targets.Count == 0)
            {
                yield break;
            }
            RoomState room = cardEffectParams.targets[0].GetCurrentRoom();
            if (room == null)
            {
                yield break;
            }
            foreach (CharacterState target in cardEffectParams.targets)
            {
                if (target.GetTeamType() == Team.Type.Monsters)
                    yield return combatManager.RunUnitTurn(target, monsterManager, heroManager, room.GetRoomIndex(), allowTheft: false);
                else if (target.GetTeamType() == Team.Type.Heroes)
                    yield return combatManager.RunUnitTurn(target, heroManager, monsterManager, room.GetRoomIndex(), allowTheft: false);
            }
            if (!coreGameManagers.GetSaveManager().PreviewMode)
            {
                TargetHelper.AttackInfo attackInfo = new();
                room.AddCharactersToList(attackInfo.othersInRoom, Team.Type.Heroes | Team.Type.Monsters);
                yield return combatManager.DoForegroundMoveAnimations(attackInfo);
            }
        }

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return [];
        }
    }
}
