using Conductor.RoomModifiers;
using ShinyShoe.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.StatusEffects
{
    public class StatusEffectHeroicState : StatusEffectState
    {
        private RoomStateHeroicModifier? damageModifier;

        protected override void CopyStateForPreviewInternal(StatusEffectState dest, List<IRoomStateModifier> characterRoomStateModifiers)
        {
            if (dest is StatusEffectHeroicState statusEffectHeroicState)
            {
                statusEffectHeroicState.damageModifier = damageModifier?.CopyForPreview();
                int num = ((damageModifier != null) ? characterRoomStateModifiers.IndexOf(damageModifier) : (-1));
                if (num >= 0)
                {
                    characterRoomStateModifiers[num] = statusEffectHeroicState.damageModifier!;
                }
            }
        }

        public override void OnStacksAdded(CharacterState character, int numStacksAdded, CharacterState.AddStatusEffectParams addStatusEffectParams, ICoreGameManagers coreGameManagers)
        {
            if (coreGameManagers.GetSaveManager().PreviewMode == IsPreviewModeCopy())
            {
                if (damageModifier == null)
                {
                    RoomModifierData setRoomModifierData = new RoomModifierData();
                    damageModifier = new RoomStateHeroicModifier();
                    damageModifier.Initialize(setRoomModifierData, coreGameManagers);
                    damageModifier.IsPreviewModeCopy = IsPreviewModeCopy();
                    damageModifier.TeamType = character.GetTeamType();
                    character.AddNewCharacterRoomModifierState(damageModifier);
                }
                if (damageModifier != null)
                {
                    damageModifier.DamageModifier = GetAssociatedCharacter().GetStatusEffectStacks(GetStatusId());
                    RoomManager roomManager = coreGameManagers.GetRoomManager();
                    RoomState currentRoom = character.GetCurrentRoom();
                    bool selected = roomManager.CurrentSelectedRoom == currentRoom.GetRoomIndex();
                    currentRoom.UpdateRoomSelectedRoomStateModifiers(selected, coreGameManagers);
                }
            }
        }

        public override void OnStacksRemoved(CharacterState character, int numStacksRemoved, ICoreGameManagers coreGameManagers)
        {
            if (damageModifier != null)
            {
                damageModifier.DamageModifier = GetAssociatedCharacter().GetStatusEffectStacks(GetStatusId());
            }
        }
    }
}
