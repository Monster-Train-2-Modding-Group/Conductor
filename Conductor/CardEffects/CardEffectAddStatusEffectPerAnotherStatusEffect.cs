using ShinyShoe.Logging;
using System.Collections;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Similar to CardEffectAddStatusEffectPerOtherEffect except a slight behaviour change when multiple units are targetted it does it character by character getting a status the character has and giving a multiple of another.
    /// Rather than summing up everyone's (or all targets) status and giving a multiple of another.
    /// Example Give 5 stacks of armor per every stack of rage.
    /// 
    /// Test Passes if any character targeted has stacks of the first status preset in param_status_effects.
    /// 
    /// Example Json
    /// {
    ///   "id": "GiveFiveSapForEveryStackOfPyregel",
    ///   "name": {
    ///     "id": "CardEffectAddStatusEffectPerAnotherStatusEffect",
    ///     "mod_reference": "Conductor"
    ///   },
    ///   "target_mode": "drop_target_character",
    ///   "target_team": "both",
    ///   "param_status_effects": [
    ///     {
    ///       "status": "pyregel",
    ///       "count":  0
    ///     },
    ///     {
    ///       "status": "debuff",
    ///       "count": 5
    ///     }
    ///   ]
    /// }
    /// </summary>
    public class CardEffectAddStatusEffectPerAnotherStatusEffect : CardEffectAddStatusEffect
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.ParamStatusEffects.GetFieldName()] = new PropDescription("Two Status Effects", "First: Effect to check and get stacks of. Second: Effect to give along with multipler. Add the second status effect (the count of the second effect) * (count of first)."),
            };
        }

        public override void Setup(CardEffectState cardEffectState)
        {
            base.Setup(cardEffectState);
            // Sanity checks
            if (cardEffectState.GetParamStatusEffectStackData()?.Count() != 2)
            {
                Log.Warning(LogGroups.Gameplay, this.GetType().Name + ": Missing required field param_status_effects or param_status_effects doesn't contain 2 elements.");
            }
        }

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            StatusEffectStackData[] paramStatusEffects = cardEffectState.GetSourceCardEffectData().GetParamStatusEffects();
            StatusEffectStackData statusEffectToCheck = paramStatusEffects[0];
            foreach (CharacterState item in cardEffectParams.targets)
            {
                int statusEffectStacks = item.GetStatusEffectStacks(statusEffectToCheck.statusId);
                if (statusEffectStacks > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers systemManagers)
        {
            StatusEffectStackData[] paramStatusEffects = cardEffectState.GetSourceCardEffectData().GetParamStatusEffects();
            StatusEffectStackData statusEffectToCheck = paramStatusEffects[0];
            StatusEffectStackData statusEffectToGive = paramStatusEffects[1];

            if (statusEffectToGive == null)
            {
                yield break;
            }

            foreach (CharacterState item in cardEffectParams.targets)
            {
                int statusEffectStacks = item.GetStatusEffectStacks(statusEffectToCheck.statusId);
                item.AddStatusEffect(addStatusEffectParams: new CharacterState.AddStatusEffectParams
                {
                    sourceRelicState = cardEffectParams.sourceRelic,
                    sourceIsHero = (cardEffectState.GetSourceTeamType() == Team.Type.Heroes),
                    fromEffectType = this.GetType(),
                }, statusId: statusEffectToGive.statusId, numStacks: statusEffectStacks * statusEffectToGive.count, allowModification: false, isFromHiddenTrigger: cardEffectParams.isFromHiddenTrigger);
            }
        }
    }
}
