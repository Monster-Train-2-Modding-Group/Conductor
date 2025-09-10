using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Card effect that requires a number of targets specified by param_int.
    /// The main use-case of this CardEffect is to stop processing effects if this one fails.
    /// 
    /// Test fails if:
    ///    use_int_range is true and the number of targets specified is not in the range [param_min_int, param_max_int].
    ///    param_bool is true and the number of targets specified does not match the requirement.
    ///    param_bool is false and the number of targets specified is less than the requirement.
    /// 
    /// Example Json.
    /// "effects": [
    ///   {
    ///     "id": "RequireAtLeastTwoTargets",
    ///     "name": {
    ///       "id": "@CardEffectRequireNumTargets",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "target_mode": "room",
    ///     "target_team": "monsters",
    ///     "param_bool": false
    ///   }
    /// ]
    /// </summary>
    public sealed class CardEffectRequireNumTargets : CardEffectBase
    {
        private int requirement;

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions {
                [CardEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("Number of Targets Requirement"),
                [CardEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("If true, number of targets must equal requirement.")
            };
        }

        public override void Setup(CardEffectState cardEffectState)
        {
            base.Setup(cardEffectState);
            requirement = cardEffectState.GetParamInt();
        }

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            int targets = cardEffectParams.targets.Count;
            if (cardEffectState.GetUseIntRange())
            {
                return targets >= cardEffectState.GetParamMinInt() && targets <= cardEffectState.GetParamMaxInt();
            }
            return cardEffectState.GetParamBool() ? targets == requirement : targets >= requirement;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            yield break;
        }
    }
}
