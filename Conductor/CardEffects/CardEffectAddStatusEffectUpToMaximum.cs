using ShinyShoe.Logging;
using System.Collections;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Gives a status effect upto a specified maximum.
    /// Currently doesn't consider CardTraits, RoomModifiers, and Dualism.
    /// 
    /// Example Json
    /// {
    ///   "id": "GiveTwoRageButWithAMaximumOfThreeRage",
    ///   "name": {
    ///     "id": "CardEffectAddStatusEffectUpToMaximum",
    ///     "mod_reference": "Conductor"
    ///   },
    ///   "target_mode": "drop_target_character",
    ///   "target_team": "both",
    ///   "param_int": 3,
    ///   "param_status_effects": [
    ///     {
    ///       "status": "buff",
    ///       "count":  2
    ///     }
    ///   ]
    /// }
    /// </summary>
    public class CardEffectAddStatusEffectUpToMaximum : CardEffectAddStatusEffect
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("Strict Target Checking", "TRUE - Always Fail Test if Zero Targets\nFALSE - Zero Targets allowed unless Target Mode is 'Drop Target'."),
                [CardEffectFieldNames.ParamStatusEffects.GetFieldName()] = new PropDescription("Singular Status Effect To Add only provide one status effect."),
                [CardEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("Maximum amount of status the targets can have."),
                [CardEffectFieldNames.ParamSubtype.GetFieldName()] = new PropDescription("Target Subtype.")
            };
        }

        public override void Setup(CardEffectState cardEffectState)
        {
            base.Setup(cardEffectState);
            // Sanity checks
            if (cardEffectState.GetParamStatusEffectStackData()?.Count() != 1)
            {
                Log.Warning(LogGroups.Gameplay, this.GetType().Name + ": Missing required field param_status_effects or param_status_effects doesn't contain 1 element.");
            }
        }

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            // TODO run through targets and see if any of them would get a status applied for the test condition, to fail the test.
            return base.TestEffect(cardEffectState, cardEffectParams, coreGameManagers);
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers systemManagers)
        {
            StatusEffectStackData statusEffectToGive = cardEffectState.GetParamStatusStack();
            if (statusEffectToGive == null)
            {
                yield break;
            }
            int amountToGive = statusEffectToGive.count;
            int maximumAmount = cardEffectState.GetParamInt();
            foreach (CharacterState character in cardEffectParams.targets)
            {
                int statusEffectStacks = character.GetStatusEffectStacks(statusEffectToGive.statusId);
                if (statusEffectStacks > maximumAmount)
                    continue;

                int effectiveAmountToGive = Math.Min(amountToGive, maximumAmount - statusEffectStacks);
                if (effectiveAmountToGive <= 0)
                    continue;

                // TODO Handle Dualism, Traits, and RoomModifiers. Allow modification is false as you don't want to go over ParamInt, but AddStatusEffect will change the effective amount given.
                character.AddStatusEffect(addStatusEffectParams: new CharacterState.AddStatusEffectParams
                {
                    sourceRelicState = cardEffectParams.sourceRelic,
                    sourceIsHero = (cardEffectState.GetSourceTeamType() == Team.Type.Heroes),
                    fromEffectType = this.GetType()
                }, statusId: statusEffectToGive.statusId, numStacks: effectiveAmountToGive, allowModification: false, isFromHiddenTrigger: cardEffectParams.isFromHiddenTrigger);
            }
        }
    }
}
