namespace Conductor.CardTraits
{
    /// <summary>
    /// Card Trait that handles scaling a Unit's size, safely.
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
    ///     "id": "UpgradeDecreaseUnitSizeBy1xPlayedCostThisTurn",
    ///     "name": {
    ///       "id": "@CardTraitScalingUpgradeUnitSizeSafely",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "param_int": -1,
    ///     "param_tracked_value": "played_cost",
    ///     "param_entry_duration": "this_turn",
    ///     "param_upgrade": "@MyUpgrade"
    ///   }
    /// ],
    /// "upgrades": [
    ///   {
    ///     "id": "@MyUpgrade",
    ///     "bonus_size": 0
    ///   }
    /// ],
    /// "triggers": [
    ///   {
    ///     "id": "StatBonusOnSummon",
    ///     "description": "Gain [trait0.power][x][size]."
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
    public sealed class CardTraitScalingUpgradeUnitSizeSafely : CardTraitState
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardTraitFieldNames.ParamTrackedValue.GetFieldName()] = new PropDescription("(Required) TrackedValue statistic to use."),
                [CardTraitFieldNames.ParamEntryDuration.GetFieldName()] = new PropDescription("(Required) Duration for the TrackedValue statistic."),
                [CardTraitFieldNames.ParamSubtype.GetFieldName()] = new PropDescription("Subtype for the TrackedValue statistic if applicable."),
                [CardTraitFieldNames.ParamCardType.GetFieldName()] = new PropDescription("Card Type for the TrackedValue statistic if applicable."),

                [CardTraitFieldNames.ParamInt.GetFieldName()] = new PropDescription("(Required) Size multiplier applied to the TrackedValue and added BonusSize of the CardUpgrade."),
                [CardTraitFieldNames.ParamCardUpgradeData.GetFieldName()] = new PropDescription("(Required) Restrict scaling upgrades to this one provided, all other upgrades that are not this one provided will not be scaled."),
            };
        }

        public override void OnApplyingCardUpgradeToUnit(CardState thisCard, CharacterState targetUnit, CharacterTriggerState? characterTriggerState, CardUpgradeState upgradeState, ICoreGameManagers coreGameManagers)
        {
            if (GetCardUpgradeDataParam() != upgradeState.GetSourceCardUpgradeData())
            {
                return;
            }
            int additionalSize = upgradeState.GetAdditionalSize();
            int additionalSize2 = GetAdditionalSize(coreGameManagers.GetCardStatistics(), setForPreviewText: false);
            upgradeState.SetAdditionalSize(additionalSize + additionalSize2);
        }

        private int GetAdditionalSize(CardStatistics cardStatistics, bool setForPreviewText)
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
                return string.Format("CardTraitScalingUpgradeUnitSize_CurrentScaling_CardText".Localize(), GetAdditionalSize(cardStatistics, setForPreviewText: true));
            }
            return string.Empty;
        }
    }
}
