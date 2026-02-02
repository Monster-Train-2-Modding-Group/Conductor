namespace Conductor.CardTraits
{
    /// <summary>
    /// Card Trait that handles scaling a Unit's status effects, safely.
    /// This CardTrait should only be used on Cards which spawn units.
    /// The issue with using a CardTrait on a Unit card which scales is that it indiscrimanitely scales
    /// *every* upgrade the unit applies to itself (or others), which may not be the intended effect.
    /// 
    /// The famous example Kinhost Carapace + Heaven's Aid (Revenge: +1 Attack) buff.
    /// Since the Trigger added by the Heaven's Aid upgrade applies a card upgrade to units,
    /// the upgrade is modified by the Card Trait on Kinhost Carapace.
    /// 
    /// 
    /// Note that Spell cards which applies upgrades are unaffected since the CardTraits on the played
    /// card modify the upgrade applied, not the Card traits on the unit on which the upgrade is applied to.
    /// 
    /// "traits": [
    ///   {
    ///     "id": "UpgradeUnitValorStatusBy6xPlayedCostThisTurn",
    ///     "name": {
    ///       "id": "@CardTraitScalingUpgradeUnitStatusEffectSafely",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "param_int": 6,
    ///     "param_tracked_value": "played_cost",
    ///     "param_entry_duration": "this_turn",
    ///     "param_upgrade": "@MyUpgrade",
    ///     "param_status_effects" : [
    ///       {
    ///         "status": "valor",
    ///         "count": 0
    ///       }
    ///     ]
    ///   }
    /// ],
    /// "upgrades": [
    ///   {
    ///     "id": "@MyUpgrade",
    ///     "status_effect_upgrades": [
    ///       {
    ///         "status": "valor",
    ///         "count" 0
    ///       }
    ///     ]
    ///   }
    /// ],
    /// "triggers": [
    ///   {
    ///     "id": "StatBonusOnSummon",
    ///     "description": "Gain [trait0.power][x][valor] for rest the battle."
    ///     "trigger": "on_spawn",
    ///     "effects": "@StatBonus"
    ///   }
    /// ],
    /// "effects": [
    ///   {
    ///     "id": "StatBonus",
    ///     "name": "CardEffectAddTempCardUpgradeToUnits",
    ///     "target_mode": "self",
    ///     "param_upgrade": "@MyUpgrade"
    ///   }
    /// ]
    /// </summary>
    public sealed class CardTraitScalingUpgradeUnitStatusEffectSafely : CardTraitState
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardTraitFieldNames.ParamTrackedValue.GetFieldName()] = new PropDescription("(Required) TrackedValue statistic to use."),
                [CardTraitFieldNames.ParamEntryDuration.GetFieldName()] = new PropDescription("(Required) Duration for the TrackedValue statistic."),
                [CardTraitFieldNames.ParamSubtype.GetFieldName()] = new PropDescription("Subtype for the TrackedValue statistic if applicable."),
                [CardTraitFieldNames.ParamCardType.GetFieldName()] = new PropDescription("Card Type for the TrackedValue statistic if applicable."),

                [CardTraitFieldNames.ParamInt.GetFieldName()] = new PropDescription("(Required) Status multiplier applied to the TrackedValue and added BonusSize of the CardUpgrade."),
                [CardTraitFieldNames.ParamCardUpgradeData.GetFieldName()] = new PropDescription("(Required) Restrict scaling upgrades to this one provided, all other upgrades of different IDs will not be scaled."),
                [CardTraitFieldNames.ParamStatusEffects.GetFieldName()] = new PropDescription("(Required) Status Effects to add. Note that if the status effect is not present in the upgrade it will be added.")
            };
        }

        public override void OnApplyingCardUpgradeToUnit(CardState thisCard, CharacterState targetUnit, CharacterTriggerState? characterTriggerState, CardUpgradeState upgradeState, ICoreGameManagers coreGameManagers)
        {
            if (GetCardTraitData().GetCardUpgradeDataParam().GetAssetKey() != upgradeState.GetAssetName())
            {
                return;
            }
            int additionalStacks = GetAdditionalStacks(coreGameManagers.GetCardStatistics(), setForPreviewText: false);
            StatusEffectStackData[] array = GetParamStatusEffects();
            foreach (StatusEffectStackData statusEffectStackData in array)
            {
                upgradeState.AddStatusEffectUpgradeStacks(statusEffectStackData.statusId, additionalStacks);
            }
        }

        private int GetAdditionalStacks(CardStatistics cardStatistics, bool setForPreviewText)
        {
            CardStatistics.StatValueData statValueData = default;
            statValueData.cardState = GetCard();
            statValueData.trackedValue = GetParamTrackedValue();
            statValueData.entryDuration = GetParamEntryDuration();
            statValueData.cardTypeTarget = GetParamCardType();
            statValueData.paramSubtype = GetParamSubtype();
            statValueData.paramStatusEffects = GetParamStatusEffects();
            statValueData.paramTeamType = GetParamTeamType();
            statValueData.forPreviewText = setForPreviewText;
            CardStatistics.StatValueData statValueData2 = statValueData;
            int statValue = cardStatistics.GetStatValue(statValueData2);
            return GetParamInt() * statValue;
        }

        public override string GetCurrentEffectText(CardStatistics? cardStatistics, SaveManager? saveManager, RelicManager? relicManager)
        {
            if (cardStatistics != null && cardStatistics.GetStatValueShouldDisplayOnCardNow(StatValueData))
            {
                return string.Format("CardTraitScalingAddStatusEffect_CurrentScaling_CardText".Localize(), GetAdditionalStacks(cardStatistics, setForPreviewText: true));
            }
            return string.Empty;
        }
    }
}
