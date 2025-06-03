using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Card effect that requires a status threshold on units targetted.
    /// The main use-case of this CardEffect is to stop processing effects if this one fails.
    /// 
    /// Test fails if:
    ///    param_bool is true and not all of the targets meet the status effect requirement.
    ///    param_bool is false and none of the targets meet the status effect requirement.
    /// 
    /// Example Json.
    /// "effects": [
    ///   {
    ///     "id": "RequireFrontFriendlyUnitHas20Valor",
    ///     "name": {
    ///       "id": "@CardEffectRequireStatusThreshold",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "target_mode": "front_in_room",
    ///     "target_team": "monsters",
    ///     "param_bool": true,
    ///     "param_status_effects": {
    ///       "status": "valor",
    ///       "count": 20
    ///     }
    ///   }
    /// ]
    /// </summary>
    public sealed class CardEffectRequireStatusThreshold : CardEffectBase
    {
        private StatusEffectStackData? statusRequirement;

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions {
                // TODO instead of requiring just one, allow user to specify a list and have the behaviour be ANY of the status effects.
                //     If the user wants AND behaviour then just append another instance with the status.
                [CardEffectFieldNames.ParamStatusEffects.GetFieldName()] = new PropDescription("Status Effects Requirement", "First: Requirement."),
                [CardEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("All targets required to meet the status effect requirement.")
            };
        }

        public override void Setup(CardEffectState cardEffectState)
        {
            base.Setup(cardEffectState);
            StatusEffectStackData[] paramStatusEffectStackData = cardEffectState.GetParamStatusEffectStackData();
            statusRequirement = paramStatusEffectStackData[0];
        }

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            bool atLeastOneMet = false;
            if (statusRequirement != null)
            {
                foreach (CharacterState target in cardEffectParams.targets)
                {
                    if (target.GetStatusEffectStacks(statusRequirement.statusId) >= statusRequirement.count)
                        atLeastOneMet = true;
                    if (target.GetStatusEffectStacks(statusRequirement.statusId) < statusRequirement.count && cardEffectState.GetParamBool())
                    {
                        return false;
                    }
                }
            }
            return atLeastOneMet;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            yield break;
        }

        public override void GetTooltipsStatusList(CardEffectState cardEffectState, ref List<string> outStatusIdList)
        {
            GetTooltipsStatusList(cardEffectState.GetSourceCardEffectData(), ref outStatusIdList);
        }

        public static void GetTooltipsStatusList(CardEffectData cardEffectData, ref List<string> outStatusIdList)
        {
            CardEffectAddStatusEffect.GetTooltipsStatusList(cardEffectData, ref outStatusIdList);
        }
    }
}
