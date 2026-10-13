using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Conductor.CardEffects.Conditions
{
    /// <summary>
    /// Card effect that prevents activating a targeted room abiliity on a character who is not on the floor where the room ability is.
    /// </summary>
    public class CardEffectRequireRoomAbilityActivatedOnItsFloor : CardEffectBase
    {
        
        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            if (cardEffectParams.roomThatActivatedAbility == null)
                return true;

            return cardEffectParams.roomThatActivatedAbility.GetRoomIndex() == cardEffectParams.selectedRoom;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            yield break;
        }

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return [];
        }
    }
}
