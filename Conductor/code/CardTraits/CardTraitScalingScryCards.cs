using Conductor.Interfaces;
using System.Collections;

namespace Conductor.CardTraits
{
    public class CardTraitScalingScryCards : CardTraitState, ICardTraitScryState
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardTraitFieldNames.ParamTrackedValue.GetFieldName()] = new PropDescription("(Required) TrackedValue statistic to use."),
                [CardTraitFieldNames.ParamEntryDuration.GetFieldName()] = new PropDescription("(Required) Duration for the TrackedValue statistic."),
                [CardTraitFieldNames.ParamCardType.GetFieldName()] = new PropDescription("Card Type"),
                [CardTraitFieldNames.ParamSubtype.GetFieldName()] = new PropDescription("Subtype"),
                [CardTraitFieldNames.ParamStatusEffects.GetFieldName()] = new PropDescription("Status Effects"),
                [CardTraitFieldNames.ParamTeamType.GetFieldName()] = new PropDescription("Team")
            };
        }

        public int GetScryAmount(int currentScry, CardStatistics cardStatistics)
        {
            CardStatistics.StatValueData statValueData = default;
            statValueData.cardState = GetCard();
            statValueData.trackedValue = GetParamTrackedValue();
            statValueData.entryDuration = GetParamEntryDuration();
            statValueData.cardTypeTarget = GetParamCardType();
            statValueData.paramSubtype = GetParamSubtype();
            statValueData.paramStatusEffects = GetParamStatusEffects();
            statValueData.paramTeamType = GetParamTeamType();
            statValueData.paramCardData = GetParamCardData();
            statValueData.forPreviewText = false;
            CardStatistics.StatValueData statValueData2 = statValueData;
            int statValue = cardStatistics.GetStatValue(statValueData2);
            return currentScry + GetParamInt() * statValue;
        }

        public override string GetCurrentEffectText(CardStatistics? cardStatistics, SaveManager? saveManager, RelicManager? relicManager)
        {
            if (cardStatistics != null && cardStatistics.GetStatValueShouldDisplayOnCardNow(base.StatValueData))
            {
                int scryAmount = GetScryAmount(0, cardStatistics);
                return "CardTraitScalingScryCards_CurrentScaling_CardText".Localize(new LocalizedIntegers(scryAmount));
            }
            return string.Empty;
        }

        public int GetScryAmountOnScried()
        {
            return 0;
        }

        public IEnumerator OnScry(ICoreGameManagers coreGameManagers)
        {
            yield break;
        }
    }
}
