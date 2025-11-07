using ShinyShoe.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Conductor.RoomModifiers
{
    /// <summary>
    /// Not designed to be used directly. This drives the Heroic status effects implementation and is controlled by that status effect.
    /// </summary>
    public class RoomStateHeroicModifier : RoomStateModifierBase, IRoomStateModifier, IRoomStateDamageModifier, ILocalizationParamInt, ILocalizationParameterContext
    {
        public bool IsPreviewModeCopy { get; set; }
        public int DamageModifier = 0;
        public Team.Type TeamType = Team.Type.None;

        public RoomStateHeroicModifier CopyForPreview()
        {
            RoomStateHeroicModifier roomStateHeroicModifier = new();
            CopyBaseStateForPreview(roomStateHeroicModifier);
            roomStateHeroicModifier.DamageModifier = DamageModifier;
            roomStateHeroicModifier.TeamType = TeamType;
            roomStateHeroicModifier.IsPreviewModeCopy = true;
            return roomStateHeroicModifier;
        }

        public int GetModifiedAttackDamage(Damage.Type damageType, CharacterState attackerState, bool requestingForCharacterStats, ICoreGameManagers coreGameManagers)
        {
            if (attackerState.GetTeamType() == TeamType)
            {
                return DamageModifier;
            }
            return 0;
        }

        public int GetModifiedMagicPowerDamage(ICoreGameManagers coreGameManagers)
        {
            return 0;
        }

        public override bool GetShowTooltip()
        {
            return false;
        }
    }
}
