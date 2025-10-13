using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static CharacterState;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Card effect that requires the units targeted to not have a specific subtype.
    /// The main use-case of this CardEffect is to stop processing effects if this one fails.
    /// 
    /// Test fails if:
    ///    At least one target has the restricted subtype
    /// 
    /// Example Json.
    /// "effects": [
    ///   {
    ///     "id": "RequireNonChampionUnit",
    ///     "name": {
    ///       "id": "@CardEffectExcludeCharacterSubtype",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "target_mode": "drop_target_character",
    ///     "target_team": "monsters",
    ///     "param_subtype": "SubtypesData_Champion_83f21cbe-9d9b-4566-a2c3-ca559ab8ff34"
    ///   }
    /// ]
    /// </summary>
    public sealed class CardEffectExcludeCharacterSubtype : CardEffectBase
    {
        private SubtypeData? restrictedSubtype;

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions {
                [CardEffectFieldNames.ParamSubtype.GetFieldName()] = new PropDescription("Restricted Subtype", "If any target has this subtype test fails"),
            };
        }

        public override void Setup(CardEffectState cardEffectState)
        {
            base.Setup(cardEffectState);
            restrictedSubtype = cardEffectState.GetParamSubtype();
        }

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            foreach (CharacterState target in cardEffectParams.targets)
            {
                if (target.GetCharacterManager().DoesCharacterPassSubtypeCheck(target, restrictedSubtype))
                {
                    return false;
                }
            }
            return true;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            yield break;
        }
    }
}
